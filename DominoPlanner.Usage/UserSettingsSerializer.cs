using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

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

            using(XmlWriter xmlWriter = XmlWriter.Create(@"C:\Users\johan\Downloads\exporttest\UserSettings.xml", settings))
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
                using (XmlReader xmlReader = XmlReader.Create(UserSettings.UserSettingsPath))
                {
                    xmlReader.Read();
                    xmlReader.Read();
                    xmlReader.Read();
                    if (xmlReader.IsStartElement() && xmlReader.Name.Equals("UserSettings"))
                    {
                        xmlReader.Read();
                        xmlReader.Read();
                        while (xmlReader.IsStartElement())
                        {
                            string currentElement = string.Empty;
                            currentElement = xmlReader.Name;
                            List<PropertyValue> propValue = new List<PropertyValue>();
                            xmlReader.Read();
                            xmlReader.Read();
                            while (xmlReader.IsStartElement() && xmlReader.Name.Equals("Property"))
                            {
                                PropertyValue currentValue = new PropertyValue();
                                xmlReader.Read();
                                xmlReader.Read();
                                if (xmlReader.Name.Equals("PropertyName"))
                                {
                                    xmlReader.Read();
                                    currentValue.PropertyName = xmlReader.Value;
                                    xmlReader.Read();
                                }
                                xmlReader.Read();
                                xmlReader.Read();
                                if (xmlReader.Name.Equals("PropertyValue"))
                                {
                                    Type valueType = typeof(string);
                                    if (xmlReader.HasAttributes)
                                    {
                                        valueType = Type.GetType(xmlReader.GetAttribute(0));
                                    }
                                    xmlReader.Read();
                                    currentValue.Value = Convert.ChangeType(xmlReader.Value, valueType); ;
                                    xmlReader.Read();
                                }
                                propValue.Add(currentValue);
                                xmlReader.Read();
                                xmlReader.Read();
                                xmlReader.Read();
                                xmlReader.Read();
                            }
                            PropertyValues.Add(currentElement, propValue);
                            xmlReader.Read();
                            xmlReader.Read();
                        }
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
