using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VirtualizingUniformGrid
{
    public class CollectionToBind : INotifyPropertyChanged
    {

        private ObservableCollection<Student> students;
        public ObservableCollection<Student> StudentList
        {
            get { return students; }
            set
            {
                students = value;
                OnPropertyChanged();
            }
        }

        public CollectionToBind()
        {
            Student studentObj;
            StudentList = new ObservableCollection<Student>();
            Random randomObj = new Random();

            for (int i = 0; i < 37; i++)
            {
                studentObj = new Student();
                studentObj.FirstName = "First " + i;
                studentObj.MiddleName = "Middle " + i;
                studentObj.LastName = "Last " + i;
                studentObj.Photo = @"D:\Projects\VirtualizingUniformGrid\VirtualizingUniformGrid\imoticon.png";
                studentObj.Age = (uint)randomObj.Next(15, 80);

                StudentList.Add(studentObj);
            }
            OnPropertyChanged(nameof(StudentList));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string prop = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }

    public class Student
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public uint Age { get; set; }
        public string Photo { get; set; }
    }
}
