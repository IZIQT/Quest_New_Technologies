using Quest_New_Technologies_2.ViewModel;
using System;

namespace Quest_New_Technologies_2.Model
{
    class MainWindowDataGridModel:ViewModelBase
    {
        //        Date	Object A	Type A	Object B	Type B	Direction	Color	Intensity	LongitudeA	LongitudeA	LatitudeB	LongitudeB

        private int id;
        public int Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        private DateTime loc_Date;
        public DateTime Loc_Date
        {
            get => loc_Date;
            set
            {
                loc_Date = value;
                OnPropertyChanged(nameof(Loc_Date));
            }
        }

        private string object_A;
        public string Object_A
        {
            get => object_A;
            set
            {
                object_A = value;
                OnPropertyChanged(nameof(Object_A));
            }
        }

        private string type_A;
        public string Type_A
        {
            get => type_A;
            set
            {
                type_A = value;
                OnPropertyChanged(nameof(Type_A));
            }
        }

        private string object_B;
        public string Object_B
        {
            get => object_B;
            set
            {
                object_B = value;
                OnPropertyChanged(nameof(Object_B));
            }
        }

        private string type_B;
        public string Type_B
        {
            get => type_B;
            set
            {
                type_B = value;
                OnPropertyChanged(nameof(Type_B));
            }
        }

        private string direction;
        public string Direction
        {
            get => direction;
            set
            {
                direction = value;
                OnPropertyChanged(nameof(Direction));
            }
        }

        private string color;
        public string Color
        {
            get => color;
            set
            {
                color = value;
                OnPropertyChanged(nameof(Color));
            }
        }

        private int intensity;
        public int Intensity
        {
            get => intensity;
            set
            {
                intensity = value;
                OnPropertyChanged(nameof(Intensity));
            }
        }

        private double latitudeA;
        public double LatitudeA
        {
            get => latitudeA;
            set
            {
                latitudeA = value;
                OnPropertyChanged(nameof(LatitudeA));
            }
        }

        private double longitudeA;
        public double LongitudeA
        {
            get => longitudeA;
            set
            {
                longitudeA = value;
                OnPropertyChanged(nameof(LongitudeA));
            }
        }

        private double longitudeB;
        public double LongitudeB
        {
            get => longitudeB;
            set
            {
                longitudeB = value;
                OnPropertyChanged(nameof(LongitudeB));
            }
        }

        private double latitudeB;
        public double LatitudeB
        {
            get => latitudeB;
            set
            {
                latitudeB = value;
                OnPropertyChanged(nameof(LatitudeB));
            }
        }

    }
}
