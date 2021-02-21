using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace DominoPlanner.Usage
{
    public class UserSettingsSerializer
    {
        private UserSettingsSerializer()
        {
            PropertyValues = new Dictionary<string, List<PropertyValue>>();

        }

        private static UserSettingsSerializer _Instance;

        public static UserSettingsSerializer Instance
        {
            get
            {
                if(_Instance == null)
                {
                    _Instance = new UserSettingsSerializer();
                    _Instance.LoadUserSettings();
                }
                return _Instance;
            }
        }

        public void SaveSettings()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;

            using(XmlWriter xmlWriter = XmlWriter.Create(UserSettings.UserSettingsPath, settings))
            {
                xmlWriter.WriteStartElement("UserSettings");

                foreach (KeyValuePair<string, List<PropertyValue>> keyValue in PropertyValues)
                {
                    xmlWriter.WriteStartElement(keyValue.Key);
                    foreach (PropertyValue propValue in keyValue.Value)
                    {
                        xmlWriter.WriteStartElement("Property");
                        xmlWriter.WriteElementString("PropertyName", propValue.PropertyName);
                        xmlWriter.WriteStartElement("PropertyValue");
                        xmlWriter.WriteAttributeString("DataType", propValue.Value.GetType().ToString());
                        xmlWriter.WriteValue(propValue.Value);
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
                xmlWriter.Close();
            }
        }
        public void LoadUserSettings()
        {
            if (File.Exists(UserSettings.UserSettingsPath))
            {
                XDocument xDocument = XDocument.Load(UserSettings.UserSettingsPath);
                if (xDocument.FirstNode != null && xDocument.FirstNode is XElement rootNode && rootNode.Name.LocalName.Equals("UserSettings"))
                {
                    foreach (XElement classElement in rootNode.Nodes().ToList().OfType<XElement>())
                    {
                        string currentElement = classElement.Name.LocalName;
                        List<PropertyValue> propValue = new List<PropertyValue>();
                        foreach (XElement propertyElement in classElement.Nodes().ToList().OfType<XElement>())
                        {
                            PropertyValue currentValue = new PropertyValue();
                            foreach (XElement propertyValue in propertyElement.Nodes().ToList().OfType<XElement>())
                            {
                                if (propertyValue.Name.LocalName.Equals("PropertyName"))
                                {
                                    if (propertyValue.FirstNode is XText propName)
                                    {
                                        currentValue.PropertyName = propName.Value;
                                    }
                                }
                                else if (propertyValue.Name.LocalName.Equals("PropertyValue"))
                                {
                                    if (propertyValue.FirstNode is XText pValue)
                                    {
                                        Type valueType = typeof(string);
                                        if (propertyValue.FirstAttribute is XAttribute attribute)
                                        {
                                            valueType = Type.GetType(attribute.Value);
                                        }
                                        currentValue.Value = Convert.ChangeType(pValue.Value, valueType);
                                    }
                                }
                            }
                            propValue.Add(currentValue);
                        }
                        PropertyValues.Add(currentElement, propValue);
                    }
                }
            }
        }

        public void AddPropertyValue(string classType, string propertyName, object value)
        {
            if (!PropertyValues.ContainsKey(classType))
            {
                PropertyValues.Add(classType, new List<PropertyValue>());
            }
            if(PropertyValues[classType].Any(x => x.PropertyName.Equals(propertyName)))
            {
                PropertyValues[classType].FirstOrDefault(x => x.PropertyName.Equals(propertyName)).Value = value;
            }
            else
            {
                PropertyValues[classType].Add(new PropertyValue(propertyName, value));
            }
        }

        public object GetPropertyValue(string classType, string propertyName)
        {
            if (PropertyValues.ContainsKey(classType))
            {
                if (PropertyValues[classType].Any(x => x.PropertyName.Equals(propertyName)))
                {
                    return PropertyValues[classType].FirstOrDefault(x => x.PropertyName.Equals(propertyName)).Value;
                }
            }
            return null;
        }

        private Dictionary<string, List<PropertyValue>> PropertyValues { get; set; }
    }

    public class PropertyValue
    {
        public PropertyValue()
        {

        }
        public PropertyValue(string propertyName, object value)
        {
            PropertyName = propertyName;
            Value = value;
        }

        public string PropertyName { get; set; }

        public object Value { get; set; }
    }

}
