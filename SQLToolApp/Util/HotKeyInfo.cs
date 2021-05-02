using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SQLToolApp.Util
{
    public class HotKeyInfo
    {
        public HotKeyInfo(ModifierKeys modifier, Key key)
        {
            this.modifierKey = modifier;
            this.key = key;
        }
        public ModifierKeys modifierKey { get; set; }
        public Key key { get; set; }
    }

    public static class HotKeyGenerate
    {
        public static HotKeyInfo GenerateHotKeyByString(string strCode)
        {
            string[] arr = Convert.ToString(strCode).Split('+');
            Key key = (Key)Enum.Parse(typeof(Key), arr.LastOrDefault());
            ModifierKeys modifier = (ModifierKeys)Enum.Parse(typeof(ModifierKeys), arr.FirstOrDefault());
            return new HotKeyInfo(modifier, key);
        }
    }
}
