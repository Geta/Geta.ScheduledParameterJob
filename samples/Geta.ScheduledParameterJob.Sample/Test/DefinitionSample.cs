using EPiServer.Core;
using EPiServer.Web.WebControls;
using Geta.ScheduledParameterJob.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Calendar = System.Web.UI.WebControls.Calendar;

namespace Geta.ScheduledParameterJob.Sample.Test
{
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
            else if (control is Calendar)
            {
                ((Calendar)control).SelectedDate = (DateTime)value;
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
            if (control is Calendar)
            {
                return ((Calendar)control).SelectedDate;
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
                Control = new Calendar { ID = "CalendarSample" }
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
}