using System.Windows.Forms;
using System.Linq;
using System;

namespace CGB.Models.Extensions
{
    public static class WebBrowserExtensions
    {
        // Get data by in HtmlElement by ElementText Enumeration
        public static string GetData(this WebBrowser wb, HtmlElement el, ElementText elText = ElementText.InnerText)
        {
            if (el.IsNull()) return "";

            string text = "";
            switch (elText)
            {
                case ElementText.InnerHtml:
                    text = el.InnerHtml ?? "";
                    break;
                case ElementText.InnerText:
                    text = el.InnerText ?? "";
                    break;
                case ElementText.OuterHtml:
                    text = el.OuterHtml ?? "";
                    break;
                case ElementText.OuterText:
                    text = el.OuterText ?? "";
                    break;
            }

            return (text ?? "").Trim();
        }

        public static string GetData(this HtmlElement el, ElementText elText = ElementText.InnerText)
        {
            if (el.IsNull()) return "";

            string text = "";
            switch (elText)
            {
                case ElementText.InnerHtml:
                    text = el.InnerHtml ?? "";
                    break;
                case ElementText.InnerText:
                    text = el.InnerText ?? "";
                    break;
                case ElementText.OuterHtml:
                    text = el.OuterHtml ?? "";
                    break;
                case ElementText.OuterText:
                    text = el.OuterText ?? "";
                    break;
            }

            return (text ?? "").Trim();
        }

        /// <summary>
        /// Enable/Disable a WebBrowser.
        /// </summary>
        /// <param name="wb">
        /// A WebBrowser instance to enable/disable.
        /// </param>
        /// <param name="enable">
        /// True to enable; otherwise, disable.
        /// </param>
        public static void EnableWebBrowser(this WebBrowser wb, bool enable = true)
        {
            if (wb.IsNull())
                return;

            ((Control)wb).Enabled = enable;
        }

        /// <summary>
        /// Replaces ".InvokeMember("click");" by calling "ClickElement" Method on HtmlElement.
        /// </summary>
        public static void ClickElement(this HtmlElement el)
        {
            if (el.IsNull())
                return;

            el.InvokeMember("click");
        }

        public static HtmlElement InvokeEvent(this HtmlElement el, string name)
        {
            if (el.IsNull())
                return el;

            el.InvokeMember(name);

            return el;
        }


        /// <summary>
        /// Set HtmlElement value by attribute name.
        /// </summary>
        /// <param name="value">
        /// The string to set in the attributeName.
        /// </param>
        /// /// <param name="attributeName">
        /// The string where the value is to be set.
        /// </param>
        //public static void SetValue(this HtmlElement el, string value, string attributeName = "value")
        //{
        //    if (el.IsNull() ||
        //        el.OuterHtml.IsNull()) 
        //        return;

        //    el.SetAttribute(attributeName, value);
        //}

        public static HtmlElement SetValue(this HtmlElement el, string value, string attributeName = "value")
        {
            if (el.IsNull() ||
                el.OuterHtml.IsNull())
                return el;

            el.SetAttribute(attributeName, value);

            return el;
        }

        /// <summary>
        /// Selects the option element by value
        /// </summary>
        /// <param name="value">
        /// The value to be searched
        /// </param>
        public static void SelectOption(this HtmlElement el, string value)
        {
            if (el.IsNull())
                return;

            foreach (HtmlElement elItem in el.Children)
            {
                if (!elItem.IsNull() &&
                    !elItem.InnerText.IsNull() &&
                    !elItem.OuterHtml.IsNull() &&
                    elItem.InnerText == value)
                {
                    el.SetValue(elItem.GetAttributeEx());
                    elItem.SetAttribute("selected", "selected");
                    elItem.ClickElement();
                    el.InvokeMember("onchange");

                    break;
                }
            }
        }

        public static void SelectOption(this HtmlElement el, int index)
        {
            if (el.IsNull())
                return;
            
            for (int i = 0; i < el.Children.Count; i++)
            {
                HtmlElement elItem = el.Children[i];
                if (!elItem.IsNull() &&
                    !elItem.InnerText.IsNull() &&
                    !elItem.OuterHtml.IsNull() &&
                    index == i)
                {
                    el.SetValue(elItem.GetAttributeEx());
                    elItem.SetAttribute("selected", "selected");
                    elItem.ClickElement();
                    el.InvokeMember("onchange");

                    break;
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public static string GetAttributeEx(this HtmlElement el, string attributeName = "value")
        {
            if (el.IsNull() ||
                el.OuterHtml.IsNull())
                return "";

            return el.GetAttribute(attributeName);
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool HasAttributeData(this HtmlElement el, string attributeName, string value, bool contains = false)
        {
            if (el.IsNull() ||
                el.OuterHtml.IsNull())
                return false;

            string attribData = el.GetAttribute(attributeName);
            if (contains)
                return !attribData.IsNull() && attribData.Contains(value);
            else
                return !attribData.IsNull() && attribData == value;
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool ContainsAttributeData(this HtmlElement el, params ElementAttribute[] attributeAndValues)
        {
            if (el.IsNull() ||
                el.OuterHtml.IsNull() ||
                attributeAndValues.Count() == 0)
                return false;

            foreach (ElementAttribute attrib in attributeAndValues)
                if (el.HasAttributeData(attrib.Key, attrib.Value))
                    return true;

            return false;
        }

        /// <summary>

        /// Check if data is contained in the HtmlElement by ElementText Enumeration
        /// </summary>
        public static bool ContainsData(this WebBrowser wb, HtmlElement el, string value, ElementText elText = ElementText.InnerText)
        {
            //if (wb.IsNull())
            //    throw new ArgumentNullException("wb", "Web Browser is null");

            return wb.GetData(el, elText).Contains(value);
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool ContainsData(this WebBrowser wb, ElementText elText, params string[] values)
        {
            //if (wb.IsNull())
            //    throw new ArgumentNullException("wb", "Web Browser is null");

            if (values.Length == 0)
                return false;

            foreach (string value in values.ToList())
                if (!wb.DocText(elText).ToLower().Contains(value.ToLower()))
                    return false;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool ContainsStyle(this HtmlElement el, string value)
        {
            if (el.IsNull() ||
                el.Style.IsNull())
                return false;

            return el.Style.ToLower().Contains(value.ToLower());
        }

        public static HtmlElement ContainsStyleEx(this HtmlElement el, string value)
        {
            if (el.IsNull() ||
                el.Style.IsNull())
                return null;

            if (el.Style.ToLower().Contains(value.ToLower()))
                return el;
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void SetStyle(this HtmlElement el, string value)
        {
            if (el.IsNull() ||
                el.Style.IsNull())
                return;

            el.Style = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public static string DocText(this WebBrowser wb, ElementText elText = ElementText.InnerText)
        {
            if (wb.Document.IsNull() ||
                wb.Document.Body.IsNull())
                return "";

            return wb.Document.Body.GetData(elText);
        }

        /// <summary>
        /// 
        /// </summary>
        public static HtmlElement GetElementByID(this WebBrowser wb, string value)
        {
            if (wb.IsNull() ||
                wb.Document.IsNull())
                return null;

            return wb.Document.GetElementById(value);
        }

        /// <summary>
        /// 
        /// </summary>
        public static HtmlElement GetElementByIDEx(this WebBrowser wb, string value)
        {
            if (wb.IsNull() ||
                wb.Document.IsNull())
                return null;

            HtmlElement elResult = wb.GetElementByID(value);
            if (!elResult.IsNull())
                return elResult;
            else
            {
                var frames = wb.Document.Window.Frames.OfType<HtmlWindow>().ToList();
                if (frames.Count > 0)
                {
                    for (int i = 0; i <= frames.Count - 1; i++)
                    {
                        elResult = frames[i].GetElementByID(value);
                        if (elResult != null)
                            return elResult;

                        var frame = frames[i].Document.Window.Frames.OfType<HtmlWindow>().ToList();
                        if (frame.Count > 0)
                        {
                            frames.AddRange(frame);
                        }
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        public static HtmlElement GetElementByTagEx(this WebBrowser wb, string tagName, string attributeName, string value, bool contains = false)
        {
            if (wb.IsNull() ||
                wb.Document.IsNull())
                return null;

            HtmlElement elResult = wb.GetElementByTag(tagName, attributeName, value, contains);
            if (!elResult.IsNull())
                return elResult;
            else
            {
                var frames = wb.Document.Window.Frames.OfType<HtmlWindow>().ToList();
                if (frames.Count > 0)
                {
                    for (int i = 0; i <= frames.Count - 1; i++)
                    {
                        elResult = frames[i].GetElementByTag(tagName, attributeName, value, null, contains);
                        if (elResult != null)
                            return elResult;

                        var frame = frames[i].Document.Window.Frames.OfType<HtmlWindow>().ToList();
                        if (frame.Count > 0)
                        {
                            frames.AddRange(frame);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public static HtmlElement GetElementByTagEx(this WebBrowser wb, string tagName, string value)
        {
            if (wb.IsNull() ||
                wb.Document.IsNull())
                return null;

            HtmlElement elResult = wb.GetElementByTag(tagName, value);
            if (!elResult.IsNull())
                return elResult;
            else
            {
                var frames = wb.Document.Window.Frames.OfType<HtmlWindow>().ToList();
                if (frames.Count > 0)
                {
                    for (int i = 0; i <= frames.Count - 1; i++)
                    {
                        elResult = frames[i].GetElementByTag(tagName, value);
                        if (elResult != null)
                            return elResult;

                        var frame = frames[i].Document.Window.Frames.OfType<HtmlWindow>().ToList();
                        if (frame.Count > 0)
                        {
                            frames.AddRange(frame);
                        }
                    }
                }
            }

            return null;
        }


        public static HtmlElement GetElementByTag(this HtmlWindow elFrame, string tagName, string value)
        {
            if (elFrame.IsNull() ||
                elFrame.Document.IsNull())
                return null;

            HtmlElementCollection elCol = elFrame.Document.GetElementsByTagName(tagName);

            foreach (HtmlElement elItem in elCol)
                if (elItem.GetData().Contains(value))
                    return elItem;

            return null;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public static HtmlElement GetElementByID(this HtmlElement el, string value)
        {
            if (el.IsNull() ||
                el.Document.IsNull())
                return null;

            return el.Document.GetElementById(value);
        }

        /// <summary>
        /// 
        /// </summary>
        public static HtmlElement GetElementByID(this HtmlWindow elFrame, string value)
        {
            if (elFrame.IsNull() ||
                elFrame.Document.IsNull())
                return null;

            return elFrame.Document.GetElementById(value);
        }

        /// <summary>
        /// 
        /// </summary>
        public static HtmlElement GetParent(this HtmlElement el)
        {
            if (el.IsNull() ||
                el.Parent.IsNull())
                return null;

            return el.Parent;
        }

        /// <summary>
        /// 
        /// </summary>
        public static HtmlElement GetElementByID(this WebBrowser wb, string value, int childrenCount)
        {
            if (wb.IsNull() ||
                wb.Document.IsNull())
                return null;

            HtmlElement elResult = wb.GetElementByID(value);
            return !elResult.IsNull() && elResult.Children.Count >= childrenCount
                    ? elResult
                    : null;
        }

        /// <summary>
        /// 
        /// </summary>
        public static HtmlElement GetElementInCollection(this WebBrowser wb, HtmlElementCollection elCol, string value, ElementText elText = ElementText.InnerText)
        {
            if (wb.IsNull() ||
                elCol.IsNull())
                return null;

            foreach (HtmlElement elItem in elCol)
                if (wb.GetData(elItem, elText).Contains(value))
                    return elItem;

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public static HtmlElement GetElementByTag(this WebBrowser wb, string tagName, string attributeName, string value, HtmlElement el = null)
        {
            if (wb.IsNull() ||
                wb.Document.IsNull())
                return null;

            HtmlElementCollection elCol = el.IsNull()
                                          ? wb.Document.GetElementsByTagName(tagName)
                                          : el.GetElementsByTagName(tagName);

            foreach (HtmlElement elItem in elCol)
                if (elItem.HasAttributeData(attributeName, value))
                    return elItem;

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public static HtmlElement GetElementByTag(this WebBrowser wb, string tagName, string attributeName, string value, bool contains)
        {
            if (wb.IsNull() ||
                wb.Document.IsNull())
                return null;

            HtmlElementCollection elCol = wb.Document.GetElementsByTagName(tagName);

            foreach (HtmlElement elItem in elCol)
                if (elItem.HasAttributeData(attributeName, value, contains))
                    return elItem;

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public static HtmlElement GetElementByTag(this HtmlElement el, string tagName, string attributeName, string value, bool contains = false)
        {
            if (el.IsNull())
                return null;

            HtmlElementCollection elCol = el.GetElementsByTagName(tagName);

            foreach (HtmlElement elItem in elCol)
                if (elItem.HasAttributeData(attributeName, value, contains))
                    return elItem;
            
            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        public static HtmlElement GetElementByTag(this HtmlWindow elFrame, string tagName, string attributeName, string value, HtmlElement el = null)
        {
            if (elFrame.IsNull() ||
                elFrame.Document.IsNull())
                return null;

            HtmlElementCollection elCol = el.IsNull()
                                          ? elFrame.Document.GetElementsByTagName(tagName)
                                          : el.GetElementsByTagName(tagName);

            foreach (HtmlElement elItem in elCol)
                if (elItem.HasAttributeData(attributeName, value))
                    return elItem;

            return null;
        }

        public static HtmlElement GetElementByTag(this HtmlWindow elFrame, string tagName, string attributeName, string value, HtmlElement el = null, bool contains = false)
        {
            if (elFrame.IsNull() ||
                elFrame.Document.IsNull())
                return null;

            HtmlElementCollection elCol = el.IsNull()
                                          ? elFrame.Document.GetElementsByTagName(tagName)
                                          : el.GetElementsByTagName(tagName);

            foreach (HtmlElement elItem in elCol)
                if (elItem.HasAttributeData(attributeName, value, contains))
                    return elItem;

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        public static HtmlElement GetElementByTag(this WebBrowser wb, string tagName, string value, bool checkNextSibling = false, HtmlElement el = null)
        {
            if (wb.IsNull() ||
                wb.Document.IsNull())
                return null;

            HtmlElementCollection elCol = el.IsNull()
                                          ? wb.Document.GetElementsByTagName(tagName)
                                          : el.GetElementsByTagName(tagName);

            foreach (HtmlElement elItem in elCol)
                if (wb.GetData(elItem).Contains(value))
                {
                    if (checkNextSibling)
                    {
                        if (!elItem.NextSibling.IsNull())
                            return elItem;
                    }
                    else
                        return elItem;
                }

            return null;
        }

        /// <summary>
        /// Find an element with one or more attribute and values
        /// </summary>
        /// <param name="tagName">
        /// The name of the tag whose System.Windows.Forms.HtmlElement objects you wish to retrieve.
        /// </param>
        /// <param name="attributeAndValues">
        /// </param>
        /// <returns>
        /// HtmlElement
        /// </returns>
        /// <example> This sample shows how to call the SaveData method from a wireless device.
        /// <code>
        /// "<input name="qryTypeRd" class="v-pristine v-valid" type="radio" v-model="QryType" value="3"/>"
        /// wb.GetElementByTag("input", new ElementAttribute("name", "qryTypeRd"),
        ///                            new ElementAttribute("value", "3"))
        ///
        ///</code>
        ///</example>
        public static HtmlElement GetElementByTag(this WebBrowser wb, string tagName, params ElementAttribute[] attributeAndValues)
        {
            if (wb.IsNull() ||
                wb.Document.IsNull() ||
                attributeAndValues.Count() == 0)
                return null;

            int found = 0;

            HtmlElementCollection elCol = wb.Document.GetElementsByTagName(tagName);
            foreach (HtmlElement elItem in elCol)
            {
                found = 0;
                foreach (ElementAttribute attrib in attributeAndValues)
                {
                    if (elItem.HasAttributeData(attrib.Key, attrib.Value))
                    {
                        if (++found == attributeAndValues.Count())
                            return elItem;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public static HtmlElement GetElementChildByTag(this WebBrowser wb, string tagName, string attributeName, string value, int childIndex, HtmlElement el = null)
        {
            if (wb.IsNull() ||
                wb.Document.IsNull())
                return null;

            HtmlElementCollection elCol = el.IsNull()
                                          ? wb.Document.GetElementsByTagName(tagName)
                                          : el.GetElementsByTagName(tagName);

            foreach (HtmlElement elItem in elCol)
                if (elItem.HasAttributeData(attributeName, value) &&
                    elItem.Children.Count >= childIndex)
                    return elItem.Children[childIndex];

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public static HtmlWindow GetWindowFrame(this WebBrowser wb, string frameName, HtmlDocument refDoc = null)
        {
            HtmlDocument doc = refDoc.IsNull() ? wb.Document : refDoc;
            if (!doc.IsNull() &&
                !doc.Body.IsNull() &&
                !doc.Body.OuterHtml.IsNull() &&
                doc.Body.OuterHtml.Contains(frameName))
            {
                return doc.Window.Frames[frameName];
            }

            return null;
        }

        public static HtmlWindow GetWindowFrame(this HtmlWindow frame, string frameName)
        {
            if (!frame.IsNull() &&
                !frame.Document.IsNull() &&
                !frame.Document.IsNull() &&
                !frame.Document.Body.IsNull() &&
                !frame.Document.Body.OuterHtml.IsNull() &&
                frame.Document.Body.OuterHtml.Contains(frameName))
            {
                return frame.Document.Window.Frames[frameName];
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public static HtmlElement GetNextElement(this HtmlElement el)
        {
            if (el.IsNull() ||
                el.Parent.IsNull())
                return null;

            var element = el.Parent.Children.OfType<HtmlElement>();
            int index = 0;
            foreach (HtmlElement elItem in element)
            {
                if (elItem.GetData() == el.InnerText)
                    return element.ElementAtOrDefault(index + 1);
                index++;
            }

            return null;
        }


        //GetElementByTag for HtmlElement extension
        /// <summary>
        /// 
        /// </summary>
        public static HtmlElement GetElementByTag(this HtmlElement el, string tagName, string value)
        {
            if (el.IsNull())
                return null;

            HtmlElementCollection elCol = el.GetElementsByTagName(tagName);

            foreach (HtmlElement elItem in elCol)
                if (elItem.GetData().Contains(value))
                    return elItem;

            return null;
        }

        /// <summary>
        /// Check if data is contained in the HtmlElement by ElementText Enumeration
        /// </summary>
        public static bool ContainsData(this HtmlElement el, string value, ElementText elText = ElementText.InnerText)
        {
            if (el.IsNull())
                return false;

            return el.GetData(elText).Contains(value);
        }

        public static void Paste(this HtmlElement el, string value)
        {
            if (el.IsNull())
                return;

            el.SetAttribute("value", "");
            el.Focus();
            string oldValue = Clipboard.GetText();
            Clipboard.SetText(value);
            el.Document.ExecCommand("Paste", false, null);
            Clipboard.SetText(oldValue);
        }
    }

    /// <summary>
    /// ElementText determines where to get the data in HtmlElement.
    /// </summary>
    public enum ElementText
    {
        /// <summary>
        /// Points to HtmlElement's InnerHtml value.
        /// </summary>
        InnerHtml,

        /// <summary>
        /// Points to HtmlElement's InnerText value.
        /// </summary>
        InnerText,

        /// <summary>
        /// Points to HtmlElement's OuterHtml value.
        /// </summary>
        OuterHtml,

        /// <summary>
        /// Points to HtmlElement's OuterText value.
        /// </summary>
        OuterText
    }

    /// <summary>
    /// ElementAttribute is used to define a key/value pair of HtmlElement's attribute.
    /// See <see cref="GetElementByTag(this WebBrowser wb, string tagName, params ElementAttribute[] attributeAndValues)"/> implentation
    /// </summary>
    public class ElementAttribute
    {
        /// <summary>
        /// A constructor that accepts parameter for attribute name and value.
        /// </summary>
        /// <param name="key">
        /// Attribute name
        /// </param>
        /// <param name="value">
        /// Attribute value
        /// </param>
        public ElementAttribute(string key, string value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Attribute name
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Attribute value
        /// </summary>
        public string Value { get; set; }
    }
}
