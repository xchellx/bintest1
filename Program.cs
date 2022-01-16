using BinarySerialization;
using System;
using System.IO;
using System.Text;

namespace bintest1
{
    class Program
    {
        static void Main(string[] args)
        {
            Person personToSerialize = new Person()
            {
                IsEmployed = false,
                FirstName = "John",
                LastName = "Doe",
                Age = 24,
                Occupation = "None"
            };
            EmploymentStats stats = EmploymentStats.GetEmploymentStatus(personToSerialize);
            if (stats.EmploymentAccepted)
                personToSerialize.Occupation = stats.EmploymentOccupation;

            Console.WriteLine("{0} input: {1}", nameof(personToSerialize.Occupation), personToSerialize.Occupation ?? "null");
            Console.WriteLine();

            string testFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "test.txt");
            using (FileStream file = File.Create(testFile))
            {
                BinarySerializer serializer = new BinarySerializer() { Encoding = Encoding.ASCII };
                serializer.MemberSerialized += Serializer_MemberSerialized;
                serializer.Serialize(file, personToSerialize);
                serializer.MemberSerialized -= Serializer_MemberSerialized;
            }

            BinarySerializer deserializer = new BinarySerializer() { Encoding = Encoding.ASCII };
            deserializer.MemberDeserialized += Deserializer_MemberDeserialized;
            Person deserializedPerson = deserializer.Deserialize<Person>(File.ReadAllBytes(testFile));
            deserializer.MemberDeserialized -= Deserializer_MemberDeserialized;
            Console.WriteLine("{0} output: {1}", nameof(deserializedPerson.Occupation), deserializedPerson.Occupation ?? "null");
        }

        static void Deserializer_MemberDeserialized(object? sender, MemberSerializedEventArgs e)
        {
            // Bug: This should never be reached as [SerializeWhen] prevents this from being deserialized
            Console.WriteLine("Member \"{0}\" was deserialized with the value of \"{1}\".", e.MemberName, e.Value?.ToString() ?? "null");
            if (e.MemberName == nameof(Person.Occupation))
                Console.WriteLine("This shouldn't be called for member \"{0}\".", nameof(Person.Occupation));
            Console.WriteLine();
        }

        static void Serializer_MemberSerialized(object? sender, MemberSerializedEventArgs e)
        {
            // Bug: This should never be reached as [SerializeWhen] prevents this from being serialized
            Console.WriteLine("\"{0}\" was serialized with the value of \"{1}\".", e.MemberName, e.Value?.ToString() ?? "null");
            if (e.MemberName == nameof(Person.Occupation))
                Console.WriteLine("This shouldn't be called for member \"{0}\".", nameof(Person.Occupation));
            Console.WriteLine();
        }

        sealed class Person
        {
            [FieldOrder(0)]
            public bool IsEmployed { get; set; } = false;

            [FieldOrder(1)]
            [FieldEncoding("ASCII")]
            public string FirstName { get; set; } = "";

            [FieldOrder(2)]
            [FieldEncoding("ASCII")]
            public string LastName { get; set; } = "";

            [FieldOrder(3)]
            public int Age { get; set; } = 20;

            [FieldOrder(4)]
            [FieldEncoding("ASCII")]
            [SerializeWhen(nameof(IsEmployed), true)]
            public string? Occupation { get; set; } = null;
        }

        sealed class EmploymentStats
        {
            public EmploymentStats(bool acceptEmployment, string occupationForEmployment)
            {
                EmploymentAccepted = acceptEmployment;
                EmploymentOccupation = occupationForEmployment;
            }

            public bool EmploymentAccepted { get; }

            public string EmploymentOccupation { get; }

            public static EmploymentStats GetEmploymentStatus(Person person)
            {
                // Logic for getting employee status
                // ...
                return new EmploymentStats(false, "Data Analyst");
            }
        }
    }
}
