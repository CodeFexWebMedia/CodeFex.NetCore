using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace CodeFex.NetCore.Data.Collections.Dynamic
{
    public interface IDynamicCollection
    {
        IEnumerable<object> Members { get; }
        object Value { get; }
        int Count { get; }
    }

    public class DynamicCollection : DynamicObject, IDynamicCollection
    {
        public static readonly int? NewIndex = null;

        protected object Data { get; private set; }

        public bool IsList
        {
            get
            {
                return Data is List<object>;
            }

        }

        public bool IsDictionary
        {
            get
            {
                return Data is Dictionary<string, object>;
            }

        }

        public DynamicCollection()
        {
        }

        public T AsDictionary<T>() where T : DynamicCollection
        {
            if (Data is Dictionary<string, object>) return this as T;
            if (Data != null) throw new Exception(string.Concat(nameof(Data), " already created"));

            Data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            return this as T;
        }

        public T AsList<T>() where T : DynamicCollection
        {
            if (Data is List<object>) return this as T;
            if (Data != null) throw new Exception(string.Concat(nameof(Data), " already created"));

            Data = new List<object>();
            return this as T;
        }

        public T AsObject<T>(object value) where T : DynamicCollection
        {
            if (Data != null) throw new Exception(string.Concat(nameof(Data), " already created"));

            Data = value;
            return this as T;
        }

        public T CastTo<T>(DynamicCollection source) where T : DynamicCollection
        {
            if (source == null) return null;

            var result = Activator.CreateInstance(typeof(T)) as T;
            result.Data = Data;

            return this as T;
        }

        /*
        public DynamicDataObject AsDictionary()
        {
            if (Data is Dictionary<string, object>) return this;
            if (Data != null) throw new CodeFexDataException(string.Concat(nameof(Data), " already created"));

            Data = new Dictionary<string, object>();
            return this;
        }

        public DynamicDataObject AsList()
        {
            if (Data is List<object>) return this;
            if (Data != null) throw new CodeFexDataException(string.Concat(nameof(Data), " already created"));

            Data = new List<object>();
            return this;
        }
        */

        public void CopyFrom(DynamicCollection dynamicCollection, bool overwrite = true)
        {
            if (dynamicCollection == null) return;
            
            if ((Data is IDictionary<string, object>) && (dynamicCollection.Data is IDictionary<string, object>))
            {
                var target = Data as IDictionary<string, object>;
                var source = dynamicCollection.Data as IDictionary<string, object>;

                foreach (var kvp in source)
                {
                    var value = kvp.Value;
                    if (overwrite)
                    {
                        target[kvp.Key] = value;
                    }
                    else if (value != null)
                    {
                        target[kvp.Key] = value;
                    }
                }

                return;
            }

            if ((Data is IList<object>) && (dynamicCollection.Data is IList<object>))
            {
                var target = Data as IList<object>;
                var source = dynamicCollection.Data as IList<object>;

                for (var i = 0; i< source.Count; i++)
                {
                    var value = source[i];
                    if (overwrite)
                    {
                        target[i] = value;
                    }
                    else if (value != null)
                    {
                        target[i] = value;
                    }
                }

                return;
            }

            // value
            Data = dynamicCollection.Data;
        }

        public IEnumerable<object> Members
        {
            get
            {
                if (Data is IDictionary<string, object>)
                {
                    return (Data as IDictionary<string, object>).Select(kvp => kvp as object).ToArray();
                }
                if (Data is IList<object>)
                {
                    return Data as IList<object>;
                }
                return null;
            }
        }

        public object Value
        {
            get
            {
                return Data;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (Data is IDictionary<string, object>)
            {
                (Data as IDictionary<string, object>)[binder.Name] = value;
                return true;
            }
            if (Data is IList<object>)
            {
                var indexValue = binder.Name;
                if (indexValue != null && int.TryParse(indexValue.ToString(), out var index))
                {
                    if (index >= 0)
                    {
                        (Data as IList<object>).Insert(index, value);
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (Data is IDictionary<string, object>)
            {
                return (Data as IDictionary<string, object>).TryGetValue(binder.Name, out result);
            }
            if (Data is IList<object>)
            {
                var indexValue = binder.Name;
                if (indexValue != null && int.TryParse(indexValue.ToString(), out var index))
                {
                    var data = Data as IList<object>;
                    if (index >= 0 && index < data.Count)
                    {
                        result = data[index];
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            try
            {
                result = Data.GetType().InvokeMember(binder.Name, BindingFlags.InvokeMethod, null, Data, args);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public int Count
        {
            get
            {
                if (Data is IDictionary<string, object>)
                {
                    return (Data as IDictionary<string, object>).Count;
                }
                if (Data is IList<object>)
                {
                    return (Data as IList<object>).Count;
                }

                return Data != null ? 1 : 0;
            }
        }

        public object this[string key]
        {
            get
            {
                object result = null;
                if (key != null)
                {
                    if (Data is IDictionary<string, object>)
                    {
                        if ((Data as IDictionary<string, object>).TryGetValue(key, out result))
                        {
                            return result;
                        }
                    }
                }
                return result;
            }

            set
            {
                if (key != null)
                {
                    if (Data is IDictionary<string, object>)
                    {
                        (Data as IDictionary<string, object>)[key] = value;
                    }
                }
            }
        }

        public object this[int? index]
        {
            get
            {
                if (Data is IList<object>)
                {
                    if (index != null)
                    {
                        return (Data as IList<object>)[index.Value];
                    }
                }
                return null;
            }

            set
            {
                if (Data is IList<object>)
                {
                    if (index != null)
                    {
                        (Data as IList<object>).Insert(index.Value, value);
                    }
                    else
                    {
                        (Data as IList<object>).Add(value);
                    }
                }
            }
        }

        public object this[object index]
        {
            get
            {
                if (index is string)
                {
                    return this[index as string];
                }
                if (index is int)
                {
                    return this[index as int?];
                }
                return null;
            }

            set
            {
                if (index is int)
                {
                    this[index as int?] = value;
                }
                if (index is null)
                {
                    this[NewIndex] = value;
                }
            }
        }

        public void Remove(object index)
        {
            if (index == null) return;

            if (index is string)
            {
                if (Data is IDictionary<string, object>)
                {
                    (Data as IDictionary<string, object>).Remove(index as string);
                    return;
                }
            }

            if (Data is IList<object>)
            {
                (Data as IList<object>).Remove(index);
            }
        }
    }
}
