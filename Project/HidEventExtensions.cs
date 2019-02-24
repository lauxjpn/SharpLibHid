using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using SharpLib.Hid;
using SharpLib.Win32;

namespace HidDemo
{
    public static class HidEventExtensions
    {
        /// <summary>
        /// Create a list view item describing this HidEvent
        /// </summary>
        /// <returns></returns>
        public static ListViewItem ToListViewItem(this Event hidEvent)
        {
            string usageText = "";
            string inputReport = null;

            foreach (ushort usage in hidEvent.Usages)
            {
                if (usageText != "")
                {
                    //Add a separator
                    usageText += ", ";
                }

                //Try to get a name for that usage
                string name = "";
                if (Enum.IsDefined(typeof(SharpLib.Hid.UsagePage), hidEvent.UsagePage))
                {
                    UsagePage usagePage = (UsagePage)hidEvent.UsagePage;

                    try
                    {
                        name = Enum.GetName(Utils.UsageType(usagePage), usage);
                    }
                    catch
                    {

                    }
                }

                if (name == null || name.Equals("") || hidEvent.Device.DeviceType == DeviceType.Gamepad) //Gamepad buttons do not belong to Usage enumeration, they are just ordinal
                {
                    name = usage.ToString("X2");
                }
                usageText += name;
            }

            // Get input report for generic HID events
            if (hidEvent.IsGeneric)
            {
                inputReport = hidEvent.InputReportString();
            }

            //If we are a gamepad display axis and dpad values
            if (hidEvent.Device != null && hidEvent.Device.DeviceType == DeviceType.Gamepad)
            {
                //uint dpadUsageValue = GetUsageValue((ushort)Hid.UsagePage.GenericDesktopControls, (ushort)Hid.Usage.GenericDesktop.HatSwitch);
                //usageText = dpadUsageValue.ToString("X") + " (dpad), " + usageText;

                if (usageText != "")
                {
                    //Add a separator
                    usageText += " (Buttons)";
                }

                if (usageText != "")
                {
                    //Add a separator
                    usageText += ", ";
                }

                usageText += hidEvent.GetDirectionPadState().ToString();

                //For each axis
                foreach (KeyValuePair<HIDP_VALUE_CAPS, uint> entry in hidEvent.UsageValues)
                {
                    if (entry.Key.IsRange)
                    {
                        continue;
                    }

                    //Get our usage type
                    Type usageType = Utils.UsageType((UsagePage)entry.Key.UsagePage);
                    if (usageType == null)
                    {
                        //Unknown usage type
                        //TODO: check why this is happening on Logitech rumble gamepad 2.
                        //Probably some of our axis are hiding in there.
                        continue;
                    }

                    //Get the name of our axis
                    string name = Enum.GetName(usageType, entry.Key.NotRange.Usage);

                    if (usageText != "")
                    {
                        //Add a separator
                        usageText += ", ";
                    }
                    usageText += entry.Value.ToString("X") + " (" + name + ")";
                }
            }
            //Handle keyboard events
            else if (hidEvent.IsKeyboard)
            {
                //Get the virtual key
                System.Windows.Forms.Keys vKey = (Keys)hidEvent.RawInput.keyboard.VKey;
                usageText = vKey.ToString() + " -";

                //Get the key flag
                if (hidEvent.IsButtonUp)
                {
                    usageText += " UP";
                }
                else if (hidEvent.IsButtonDown)
                {
                    usageText += " DOWN";
                }

                if (hidEvent.RawInput.keyboard.Flags.HasFlag(RawInputKeyFlags.RI_KEY_E0))
                {
                    usageText += " E0";
                }

                if (hidEvent.RawInput.keyboard.Flags.HasFlag(RawInputKeyFlags.RI_KEY_E1))
                {
                    usageText += " E1";
                }

                if (hidEvent.HasModifierShift)
                {
                    usageText += " SHIFT";
                }

                if (hidEvent.HasModifierControl)
                {
                    usageText += " CTRL";
                }

                if (hidEvent.HasModifierAlt)
                {
                    usageText += " ALT";
                }

                if (hidEvent.HasModifierWindows)
                {
                    usageText += " WIN";
                }


                //Put our scan code into our input report field
                inputReport = "0x" + hidEvent.RawInput.keyboard.MakeCode.ToString("X4");
            }

            //Now create our list item
            ListViewItem item = new ListViewItem(new[] { usageText, inputReport, hidEvent.UsagePageNameAndValue(), hidEvent.UsageCollectionNameAndValue(), hidEvent.RepeatCount.ToString(), hidEvent.Time.ToString("HH:mm:ss:fff"), hidEvent.IsBackground.ToString() });
            return item;
        }
    }
}