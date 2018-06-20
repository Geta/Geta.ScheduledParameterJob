# Scheduled job with parameters
=============================

## Description
EPiServer scheduled job with parameters that can be configured in the admin UI.

## Features
* Create a scheduled job with parameters to be used in the admin UI.
* Create any parameter type that you wish and use it within your scheduled job.
* Scheduled job will work and run like any other.

## How to get started?
* ``install-package Geta.ScheduledParameterJob``

* Add an App_Browsers folder to your site root if there is none there already.
Inside the folder you should create an AdapterMappings.browser file with the content inside:
```
<browsers>
  <browser refID="Default">
    <controlAdapters>
      <adapter controlType="EPiServer.UI.Admin.DatabaseJob"
               adapterType="Geta.ScheduledParameterJob.DatabaseJobAdapter" />
    </controlAdapters>
  </browser>
</browsers>
```

* Create you scheduled job extending **ScheduledJob** class.
* Create your Definitions (parameters) class implementing **IParameterDefinitions** interface.

## Code
Credits: [Mathias Kunto](https://blog.mathiaskunto.com/)

### Example scheduled job

Example includes definition class with all available parameter types. Use the appropriate ones that you need for your solution.
``
Make sure that you reference the correct DefinitionsClass and DefinitionsAssembly in your scheduled job's attribute. It should match the definitions class that you create to add parameters. Also dont forget to load the correct plugin descriptor. 
``

```csharp
public class DefinitionSample : IParameterDefinitions
    {
        public IEnumerable<ParameterControlDto> GetParameterControls()
        {
            return new List<ParameterControlDto>
                        {
                            AddACheckBoxSample(),
                            AddATextBoxSample(),
                            AddAnInputPageReferenceSample(),
                            AddACalendarSample(),
                            AddADropDownListSample()
                        };
        }

        public void SetValue(Control control, object value)
        {
            if (control is CheckBox)
            {
                ((CheckBox)control).Checked = (bool)value;
            }
            else if (control is TextBox)
            {
                ((TextBox)control).Text = (string)value;
            }
            else if (control is DropDownList)
            {
                ((DropDownList)control).SelectedValue = (string)value;
            }
            else if (control is InputPageReference)
            {
                ((InputPageReference)control).PageLink = (PageReference)value;
            }
            else if (control is System.Web.UI.WebControls.Calendar)
            {
                ((System.Web.UI.WebControls.Calendar)control).SelectedDate = (DateTime)value;
            }
        }

        public object GetValue(Control control)
        {
            if (control is CheckBox)
            {
                return ((CheckBox)control).Checked;
            }
            if (control is TextBox)
            {
                return ((TextBox)control).Text;
            }
            if (control is DropDownList)
            {
                return ((DropDownList)control).SelectedValue;
            }
            if (control is InputPageReference)
            {
                return ((InputPageReference)control).PageLink;
            }
            if (control is System.Web.UI.WebControls.Calendar)
            {
                return ((System.Web.UI.WebControls.Calendar)control).SelectedDate;
            }
            return null;
        }

        private static ParameterControlDto AddACheckBoxSample()
        {
            return new ParameterControlDto
            {
                // Omitting LabelText will render control without label
                Description = "Sample of a CheckBox control",
                Control = new CheckBox
                {
                    ID = "CheckBoxSample",
                    Text = "CheckBox Sample"
                }
            };
        }

        private static ParameterControlDto AddATextBoxSample()
        {
            return new ParameterControlDto
            {
                LabelText = "TextBox Sample",
                Description = "Sample of a TextBox control",
                Control = new TextBox { ID = "TextBoxSample" }
            };
        }

        private static ParameterControlDto AddAnInputPageReferenceSample()
        {
            return new ParameterControlDto
            {
                LabelText = "InputPageReference Sample",
                Description = "Sample of an EPiServer Page Selector control; InputPageReference.",
                Control = new InputPageReference { ID = "InputPageReferenceSample" }
            };
        }

        private static ParameterControlDto AddACalendarSample()
        {
            return new ParameterControlDto
            {
                LabelText = "Calendar Sample",
                Description = "Sample of a Calendar control",
                Control = new System.Web.UI.WebControls.Calendar { ID = "CalendarSample" }
            };
        }

        private static ParameterControlDto AddADropDownListSample()
        {
            return new ParameterControlDto
            {
                LabelText = "DropDownList Sample",
                Description = "Sample of a DropDownList control",
                Control = new DropDownList
                {
                    ID = "DropDownListSample",
                    DataTextField = "Text",
                    DataValueField = "Value",
                    DataSource = new List<ListItem>
                                {
                                    new ListItem
                                        {
                                            Text = "The first sample item",
                                            Value = "1"
                                        },
                                    new ListItem
                                        {
                                            Text = "The second sample item",
                                            Value = "2"
                                        },
                                    new ListItem
                                        {
                                            Text = "The third sample item",
                                            Value = "3"
                                        },
                                    new ListItem
                                        {
                                            Text = "The fourth sample item",
                                            Value = "4"
                                        }
                                }
                }
            };
        }
    }


[ScheduledPlugInWithParameters(
        DisplayName = "Sample parameter job",
        Description = "Sample job with parameters",
        DefinitionsClass = "AlloyDemo.Business.ScheduledJobs.ScheduledJobWithParameters.DefinitionSample",
        DefinitionsAssembly = "AlloyDemo"
    )]
    public class SampleParameterJob : ScheduledJob
    {
        public static string Execute()
        {
            var descriptor = PlugInDescriptor.Load("AlloyDemo.Business.ScheduledJobs.ScheduledJobWithParameters.SampleParameterJob", "AlloyDemo");
            var store = typeof(ScheduledJobParameters).GetStore();
            var parameters = store.LoadPersistedValuesFor(descriptor.ID.ToString(CultureInfo.InvariantCulture));

            var cbChecked = parameters.ContainsKey("CheckBoxSample") && (bool)parameters["CheckBoxSample"] ? "Aye!" : "Nay..";
            var tbText = parameters.ContainsKey("TextBoxSample") ? parameters["TextBoxSample"] as string : string.Empty;
            var sampleReference = parameters.ContainsKey("InputPageReferenceSample") ? (PageReference)parameters["InputPageReferenceSample"] : PageReference.EmptyReference;
            var samplePageName = sampleReference != null && sampleReference != PageReference.EmptyReference ? DataFactory.Instance.GetPage(sampleReference).PageName : string.Empty;
            var cDateTime = parameters.ContainsKey("CalendarSample") ? (DateTime?)parameters["CalendarSample"] : null;
            var ddlSelectedValue = parameters.ContainsKey("DropDownListSample") ? parameters["DropDownListSample"] as string : string.Empty;

            var result = string.Empty;
            result += string.Format("CheckBoxSample checked: <b>{0}</b><br />", cbChecked);
            result += string.Format("TextBoxSample text: <b>{0}</b><br />", tbText);
            result += string.Format("InputPageReferenceSample page name: <b>{0}</b> (PageId: <b>{1}</b>)<br />", samplePageName, sampleReference);
            result += string.Format("CalendarSample date: <b>{0}</b><br />", cDateTime.ToString());
            result += string.Format("DropDownListSample selected value: <b>{0}</b><br />", ddlSelectedValue);
            return result;
        }
    }
```

## More info

* [Blog for 7.5 upgrade](https://blog.mathiaskunto.com/2014/03/21/scheduled-jobs-with-input-parameters-in-episerver-7-5/)
* [Blog for WebForms](https://blog.mathiaskunto.com/2012/02/13/supplying-episerver-scheduled-jobs-with-parameters-through-admin-mode/)

## Package maintainer
https://github.com/digintsys

## Changelog
[Changelog](CHANGELOG.md)
