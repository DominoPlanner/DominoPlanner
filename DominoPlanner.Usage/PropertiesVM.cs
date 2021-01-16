using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using DominoPlanner.Usage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DominoPlanner.Usage
{
    using static Localizer;
    public class PropertiesVM : ModelBase
    {
        public object Model { get; set; }
        public List<PropertyEntryVM> Children { get; set; }
        public PropertiesVM(object model)
        {
            Model = model.GetType();
            var dummy = new DummyObjectWrapper() { DummyObject = model };
            var pi = dummy.GetType().GetProperties().First();
            var basenode = new PropertyItemVM(dummy, pi);
            basenode.LoadChildren(model, true);
            Children = basenode.Children;
        }
    }
    public class DummyObjectWrapper
    {
        public object DummyObject { get; set; }
    }
    public abstract class PropertyEntryVM : ModelBase
    {

        private ICommand command;

        public ICommand OpenChildren
        {
            get { return command; }
            set { command = value; RaisePropertyChanged(); }
        }
        private List<PropertyEntryVM> propertyItemVMs;

        public List<PropertyEntryVM> Children
        {
            get { return propertyItemVMs; }
            set { propertyItemVMs = value; RaisePropertyChanged(); }
        }
        public object Value { get; set; }
        public bool CanWrite { get; set; }
        private string type;

        public string Type
        {
            get { return type; }
            set { type = value; RaisePropertyChanged(); }
        }

        private string fullType;

        public string FullType
        {
            get { return fullType; }
            set { fullType = value; RaisePropertyChanged(); }
        }


        public string Name { get; set; }
        private bool hasChildren;

        public bool HasChildren
        {
            get { return hasChildren; }
            set { hasChildren = value; RaisePropertyChanged();  }
        }

        private bool isExpanded;

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (!isExpanded && value)
                {
                    LoadChildren(Value, true);
                }
                isExpanded = value; 
                RaisePropertyChanged();
            }
        }


        public PropertyEntryVM()
        {
            OpenChildren = new RelayCommand(o => LoadChildren(Value, (bool)o));
        }

        public static List<PropertyEntryVM> LoadChildren(IEnumerable<MemberInfo> ChildPI, object parent, string name)
        {
            var result = new List<PropertyEntryVM>();
            if (parent is System.Collections.IList list)
            {
                int index = 0;
                foreach (var mi in list)
                {
                    result.Add(new ArrayPropertyItem(parent, index, name));
                    index++;
                }
            }
            else
            {
                foreach (var mi in ChildPI)
                {
                    result.Add(new PropertyItemVM(parent, mi));
                }
            }
            return result;
        }
        public void PrepareTree()
        {
            if (!(Value is Exception || Value is System.Collections.IList ||
                Value == null ||
                Value.GetType().IsSimpleType()))
            {
                GetSubMembers();
            }
            if (ChildPI?.Count() > 0 || (Value is System.Collections.IList list && list.Count > 0))
            {
                HasChildren = true;
                if (Value is System.Collections.IList list2)
                {
                    
                    FullType = FullType.Replace("[]", "") + "[" + list2.Count + "]";
                }
            }

        }

        private IEnumerable<MemberInfo> ChildPI;
        public void GetSubMembers()
        {
            if (!(Value is System.Collections.IList list))
            {
                var childPropInfo = Value.GetType().GetProperties().Where(x => x.CanRead);
                var childFieldInfo = Value.GetType().GetFields();
                ChildPI = childPropInfo.Concat<MemberInfo>(childFieldInfo)
                    .Where(x => x.IsPublic() && !x.IsSubtypeOf(typeof(MulticastDelegate))
                                             && !x.IsSubtypeOf(typeof(EventHandler))
                                             && !x.IsSubtypeOf(typeof(ICommand)))
                    .OrderBy(x => x.Name);
            }
        }
        public void LoadChildren(object parent, bool check = true)
        {
            if (check && HasChildren && Children == null)
            {
                Children = LoadChildren(ChildPI, parent, Name);
            }
        }
    }

    public class PropertyItemVM : PropertyEntryVM
    {
        MemberInfo pi;
        public PropertyItemVM(object parent, MemberInfo pi)
        {
            this.pi = pi;
            Name = pi.Name;
            Type = "Field";
            CanWrite = true;
            if (pi is PropertyInfo p)
            {
                Type = "Prop";
                CanWrite = p.CanWrite;
            }
            Value = Eval(parent);
            if (!(Value is Exception) && Value != null)
            {
                FullType = Value.GetType().Name;
            }
            PrepareTree();
        }



        public object Eval(object parent)
        {
            try
            {
                if (pi is PropertyInfo p && p.CanRead)
                {
                    // in case evaluation leads to an exception, store the property type first
                    FullType = p.PropertyType.Name;

                    return p.GetValue(parent);
                    

                }
                else if (pi is FieldInfo f)
                {
                    FullType = f.FieldType.Name;
                    return f.GetValue(parent);
                }
            }
            catch (Exception ex)
            {
                return ex;
            }
            return null;
        }
    }
    public class ArrayPropertyItem : PropertyEntryVM
    {
        public int index { get; set; }
        // parent is IList
        public ArrayPropertyItem(object parent, int index, string parentname) : base()
        {
            this.index = index;
            Value = Eval(parent);
            FullType = Value.GetType().FullName;
            CanWrite = true;
            Type = "Item";
            Name = "[" + index + "]";
            PrepareTree();
        }

        public object Eval(object parent)
        {
            try
            {
                return (parent as System.Collections.IList)[index];
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
    public class DummyPropertyItem : PropertyEntryVM
    {
        public DummyPropertyItem() : base()
        {

        }
    }
    public class LevelToIndentConverter : IValueConverter
    {
        public object Convert(object o, Type type, object parameter,
                              CultureInfo culture)
        {
            return new Thickness((int)o * c_IndentSize, 0, 0, 0);
        }

        public object ConvertBack(object o, Type type, object parameter,
                                  CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private const double c_IndentSize = 19.0;
    }
    public class ObjectToStringConverter : IValueConverter

    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "null";
            switch (value)
            {
                case Exception ex:
                    return string.Format(_("Exception of type {0}"), ex.GetType()) + ":\n" + ex.StackTrace;
                case System.Collections.IList ie:
                    var type = ie.GetType();
                    var membertype = (type.GenericTypeArguments.Count() > 0 ? type.GenericTypeArguments[0] : null) ?? type.GetElementType();
                    return membertype?.ToString() + "[" + ie.Count + "]";
                default:
                    return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BooleanAndConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return values.OfType<IConvertible>().All(System.Convert.ToBoolean);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public static class TypeExtensions
    {
        /// <summary>
        /// Determine whether a type is simple (String, Decimal, DateTime, etc) 
        /// or complex (i.e. custom class with public properties and methods).
        /// </summary>
        /// <see cref="http://stackoverflow.com/questions/2442534/how-to-test-if-type-is-primitive"/>
        public static bool IsSimpleType(
            this Type type)
        {
            return
                type.IsValueType ||
                type.IsPrimitive ||
                new Type[] {
                typeof(String),
                typeof(Decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(Guid)
                }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }
        public static bool IsPublic(this MemberInfo mi)
        {
            if (mi is PropertyInfo p)
            {
                return p.GetGetMethod() != null;
            }
            else if (mi is FieldInfo f)
            {
                return f.IsPublic;
            }
            return false;
        }
        public static bool IsSubtypeOf(this MemberInfo mi, Type type)
        {
            if (mi is PropertyInfo p)
            {
                return type.IsAssignableFrom(p.PropertyType);
            }
            else if (mi is FieldInfo f)
            {
                return type.IsAssignableFrom(f.FieldType);
            }
            return false;
        }
    }
}