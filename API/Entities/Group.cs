using System;
using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class Group
{
    /* This makes Entity Framework require that the Name is unique. It becomes the primary key */
    [Key]
    public required string Name { get; set; }
    /* User can have multiple connections if they're connecting from multiple devices
    The Connections property represents a navigation property that tells EF that each Group can have multiple Connections. 
     It does not directly store data in the Group table; rather, it defines a relationship.
     In EF Core, this automatically sets up a one-to-many relationship*/
    public ICollection<Connection> Connections { get; set; } = [];
}
