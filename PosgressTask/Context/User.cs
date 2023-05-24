namespace PosgressTask.Context;

public class User
{
    public Guid Id { get; set; }
    public string Username{get;set;}
    public string Password{get;set;}
    public List<Role> Roles{get;set;}
}

public enum Role
{
    Admin,
    User
}