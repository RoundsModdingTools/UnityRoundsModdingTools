using System;
using UnityEditor;

namespace URMT.Core.Utils {
    public class SaveableProperty<T> {
        private readonly string key;
        private readonly T defaultValue;
        public T Value {
            get => Load();
            set => Save(value);
        }
        public SaveableProperty(string key, T defaultValue = default) {
            this.key = key;
            this.defaultValue = defaultValue;

            if(!IsSupportedType(typeof(T))) {
                throw new NotSupportedException($"Type {typeof(T).Name} is not supported");
            }
        }
        public void Save(T value) {
            switch(value) {
                case string s:
                    EditorPrefs.SetString(key, s);
                    break;
                case int i:
                    EditorPrefs.SetInt(key, i);
                    break;
                case float f:
                    EditorPrefs.SetFloat(key, f);
                    break;
                case bool b:
                    EditorPrefs.SetBool(key, b);
                    break;
            }
        }
        public T Load() {
            if(EditorPrefs.HasKey(key)) {
                switch(defaultValue) {
                    case string _:
                        return (T)(object)EditorPrefs.GetString(key);
                    case int _:
                        return (T)(object)EditorPrefs.GetInt(key);
                    case float _:
                        return (T)(object)EditorPrefs.GetFloat(key);
                    case bool _:
                        return (T)(object)EditorPrefs.GetBool(key);
                }
            }
            return defaultValue;
        }

        private bool IsSupportedType(Type type) {
            return type == typeof(string) || type == typeof(int) || type == typeof(float) || type == typeof(bool);
        }
    }
}
