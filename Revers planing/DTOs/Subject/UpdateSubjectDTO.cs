namespace Revers_planing.DTOs.Subject;

public class UpdateSubjectDTO
{
    public string? Name { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Discription { get; set; }
    public List<int>? AllowedGroups { get; set; }
}


