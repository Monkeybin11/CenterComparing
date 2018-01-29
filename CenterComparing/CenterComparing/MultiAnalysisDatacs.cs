using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenterComparing
{
    public class MultiAnalysisDatacs : INotifyPropertyChanged
    {
        int _no { get; set; }
        string _name { get; set; }
        string _error { get; set; }

        public int no { get { return _no; } set { _no = value; Notify("no"); } }
        public string name { get { return _name; } set { _name = value; Notify("name"); } }
        public string error { get { return _error; } set { _error = value; Notify("error"); } }
        public string fullname;

        public MultiAnalysisDatacs(int num , string name , string fullpath)
        {
            this.no = num;
            this.name = name;
            this.fullname = fullpath;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propName)

        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
       

    }
}
