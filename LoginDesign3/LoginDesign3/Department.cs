using System.Collections.Generic;

namespace LoginDesign3
{
    public class Doctor
    {
        public string Name { get; set; }
        public string DepartmentName { get; set; }
        public Dictionary<string, string> Details { get; set; }
    }

    public class Department
    {
        public string Name { get; set; }
        public Dictionary<string, Doctor> Doctors { get; set; }
    }
}