using System.ComponentModel.DataAnnotations;

namespace Test1B.Model;

public class Animal
{
    public int IdAnimal { get; set; }
    [MaxLength(200)]
    public string Name { get; set; }
    [Range(0, Int32.MaxValue)]
    public int Amount { get; set; }
}