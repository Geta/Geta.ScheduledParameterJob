﻿using Geta.ScheduledParameterJob.Extensions;
using Geta.ScheduledParameterJob.Parameters;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.Adapters;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using EPiServer.Data.Dynamic;
using EPiServer.PlugIn;
using EPiServer.Shell.WebForms;
using EPiServer.UI.Admin;

[assembly: WebResource("Geta.ScheduledParameterJob.Style.JobParameters.css", "text/css")]
namespace Geta.ScheduledParameterJob
{
    public class DatabaseJobAdapter : PageAdapter
    {
        private const string ShowResetMessage = "ShowResetMessage";
        private const string ShowSaveMessage = "ShowSaveMessage";

        private List<Control> ParameterControls { get; set; } // Would be a pain finding them again.

        private string _pluginId;
        private string PluginId => _pluginId ?? (_pluginId = ((DatabaseJob)Control).Request.QueryString["pluginId"]);

        private Dictionary<string, object> _persistedValues;
        private Dictionary<string, object> PersistedValues => _persistedValues ?? (_persistedValues = typeof(ScheduledJobParameters).GetStore().LoadPersistedValuesFor(PluginId));

        private ScheduledPlugInWithParametersAttribute _attribute;
        private ScheduledPlugInWithParametersAttribute Attribute
        {
            get
            {
                if (_attribute == null)
                {
                    var pluginIdParsed = int.TryParse(PluginId, out var pluginId);
                    if (!pluginIdParsed) 
                        return null;

                    var descriptor = PlugInDescriptor.Load(pluginId);
                    _attribute = descriptor.GetAttribute(typeof(ScheduledPlugInWithParametersAttribute)) as ScheduledPlugInWithParametersAttribute;
                }

                return _attribute;
            }
        }

        private IParameterDefinitions _parameterDefinitions;
        private IParameterDefinitions Definitions
        {
            get
            {
                if (_parameterDefinitions == null)
                {
                    var assembly = Assembly.Load(Attribute.DefinitionsAssembly);
                    _parameterDefinitions = assembly.CreateInstance(Attribute.DefinitionsClass) as IParameterDefinitions;
                    if (_parameterDefinitions == null)
                    {
                        throw new Exception("Your DefinitionsClass must implement the IParameterDefinitions interface.");
                    }
                }
                return _parameterDefinitions;
            }
        }

        public DatabaseJobAdapter()
        {
            ParameterControls = new List<Control>();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (Attribute == null)
            {
                // Not a job with parameters
                return;
            }
            Attribute.Validate();
            Initialization();
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (Attribute == null)
            {
                // Not a job with parameters
                return;
            }
            foreach (var control in ParameterControls)
            {
                control.DataBind();
            }
        }

        private void Initialization()
        {
            AddStylesheet();
            AddParameterControls();
            DisplaySystemMessage();
        }

        private void DisplaySystemMessage()
        {
            var databaseJob = (DatabaseJob)Control;
            if (!(databaseJob.Page is WebFormsBase systemPageBase))
            {
                return;
            }
            if (databaseJob.Request.Cookies[ShowResetMessage] != null)
            {
                systemPageBase.SystemMessageContainer.Message = "Parameter values were reset to default for this scheduled job.";
                databaseJob.Response.Cookies[ShowResetMessage].Expires = DateTime.Now.AddDays(-1);
            }
            else if (databaseJob.Request.Cookies[ShowSaveMessage] != null)
            {
                systemPageBase.SystemMessageContainer.Message = "Parameter values were successfully saved for this scheduled job.";
                databaseJob.Response.Cookies[ShowSaveMessage].Expires = DateTime.Now.AddDays(-1);
            }
        }

        private void AddParameterControls()
        {
            var controls = Definitions.GetParameterControls();
            var fieldset = CreateFieldsetFor(controls);

            var generalSettings = Control.FindControlRecursively("GeneralSettings"); // Div with Settings-tab content
            generalSettings.Controls.AddAt(0, fieldset);
        }

        private Control CreateFieldsetFor(IEnumerable<ParameterControlDto> controls)
        {
            var fieldset = new HtmlGenericControl("fieldset");
            fieldset.Attributes.Add("class", "job-parameters-container");
            fieldset.Controls.Add(new HtmlGenericControl("legend") { InnerText = "Job parameters" });

            foreach (var parameterControl in controls)
            {
                SetPersistedValueFor(parameterControl); // Persisted value from DDS or definition file default if not present.
                ParameterControls.Add(parameterControl.Control);
                fieldset.Controls.Add(CreateRowFor(parameterControl));
            }
            fieldset.Controls.Add(SaveAndResetValuesButtons());
            return fieldset;
        }

        private Control SaveAndResetValuesButtons()
        {
            var container = new HtmlGenericControl("div");
            container.Attributes.Add("class", "save-and-reset-button-container");

            var resetButton = new Button
            {
                Text = "Reset values",
                ToolTip = "Resets all parameters for this scheduled job to their default values.",
                CssClass = "reset-button"
            };
            resetButton.Click += new EventHandler(ResetValues_Click);
            var resetButtonOutline = new HtmlGenericControl("span");
            resetButtonOutline.Attributes.Add("class", "epi-cmsButton");
            resetButtonOutline.Controls.Add(resetButton);
            container.Controls.Add(resetButtonOutline);

            var saveButton = new Button
            {
                Text = "Save values",
                ToolTip = "Saves all parameters for this scheduled job.",
                CssClass = "epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-Save"
            };
            saveButton.Click += new EventHandler(SaveValues_Click);
            var saveButtonOutline = new HtmlGenericControl("span");
            saveButtonOutline.Attributes.Add("class", "epi-cmsButton");
            saveButtonOutline.Controls.Add(saveButton);
            container.Controls.Add(saveButtonOutline);

            return container;
        }

        private void ResetValues_Click(object sender, EventArgs e)
        {
            var store = typeof(ScheduledJobParameters).GetStore();
            store.RemovePersistedValuesFor(PluginId);
            RefreshWithMessage(ShowResetMessage);
        }

        private void SaveValues_Click(object sender, EventArgs e)
        {
            var store = typeof(ScheduledJobParameters).GetStore();
            store.RemovePersistedValuesFor(PluginId);
            store.PersistValuesFor(PluginId, ParameterControls, c => Definitions.GetValue(c));
            RefreshWithMessage(ShowSaveMessage);
        }

        private void RefreshWithMessage(string message)
        {
            var databaseJob = ((DatabaseJob)Control);
            databaseJob.Response.SetCookie(new HttpCookie(message, "true"));
            databaseJob.Response.Redirect(databaseJob.Request.Url.ToString());
        }

        private static Control CreateRowFor(ParameterControlDto parameterControlDto)
        {
            var rowContainer = new HtmlGenericControl("div");
            rowContainer.Attributes.Add("class", "parameter-control-container");
            var control = parameterControlDto.Control;
            if (parameterControlDto.ShowLabel)
            {
                var label = new Label
                {
                    AssociatedControlID = parameterControlDto.Id,
                    Text = parameterControlDto.LabelText,
                    ToolTip = parameterControlDto.Description
                };
                rowContainer.Controls.Add(label);
            }
            else
            {
                var noLabelContainer = new HtmlGenericControl("div");
                noLabelContainer.Attributes.Add("title", parameterControlDto.Description);
                noLabelContainer.Attributes.Add("class", "control-without-label");
                noLabelContainer.Controls.Add(parameterControlDto.Control);
                control = noLabelContainer;
            }
            var controlContainer = new HtmlGenericControl("div");
            controlContainer.Attributes.Add("class", "control-container");
            controlContainer.Controls.Add(control);
            rowContainer.Controls.Add(controlContainer);
            return rowContainer;
        }

        private void SetPersistedValueFor(ParameterControlDto controlDto)
        {
            if (PersistedValues.ContainsKey(controlDto.Id))
            {
                Definitions.SetValue(controlDto.Control, PersistedValues[controlDto.Id]);
            }
        }

        private void AddStylesheet()
        {
            var cssPath = Page
                .ClientScript
                .GetWebResourceUrl(typeof(DatabaseJobAdapter), "Geta.ScheduledParameterJob.Style.JobParameters.css");
            var cssLink = new HtmlLink { Href = cssPath };
            cssLink.Attributes.Add("rel", "stylesheet");
            cssLink.Attributes.Add("type", "text/css");
            cssLink.Attributes.Add("media", "screen");
            Page.Header.Controls.Add(cssLink);
        }
    }
}
