public class StudentDto
{
    public int Id { get; set; }
    public string StudentNumber { get; set; }
    public string Name { get; set; }
    public string Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string ContactNumber { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public string Status { get; set; }
    public int ClassId { get; set; }
    public ClassDto Class { get; set; }
}

public class ClassDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Grade { get; set; }
    public int Year { get; set; }
} 