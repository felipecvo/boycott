.net rocks!

Powered by C#

configurationless, code only, .net ORM library

# Setup Provider

  ` Configuration.DatabaseProvider = new MySQLProvider("localhost", "tsukianimes9", "root", "root");`


# Synchronization process

    var sync = new Synchronizator();
    if (!sync.DatabseExists) {
        Configuration.DatabaseProvider.CreateDatabase();
    }

    if (sync.Check()) {
        sync.Sync();
    }

# Defining a class

```c#
    class Airport : Boycott.Base<Airport> {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string City { get; set; }
        public string CityCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
```


# Get item by id

```c#
    var airport = Airport.Find(1);
```


# Create an item

```c#
    var airport = new Airport() {
        Name = "Guarulhos international Airport",
        Code = "GRU",
        City = "São Paulo",
        CityCode = "SAO"
    };
    airport.Save();
```


# Complex find using LINQ

```c#
    var airports = (from a in Airport.db
                        where a.Name.Contains("guarulhos")
                        orderby a.Name
                        select a).Take(10).ToList();
```


# Get all rows

```c#
    var airport = Airport.All();
```


# Mapping a legacy table

```c#
    [NotSynchronizable]
    [Table(Name = "forum")]
    class Forum : Boycott.Base<Forum> {
        [Column(Name = "ForumId", IsPrimaryKey = true)]
        [DbType(DbType = Boycott.Migrate.DbType.Char)]
        [ColumnLimit(Limit =36)]
        [NotNullable]
        [PrimaryKey]
        public Guid Id { get; set; }
    
        [Column(Name = "PartnerId")]
        public Guid PartnerId { get; set; }
    
        [Column(Name = "CategoryId")]
        public Guid CategoryId { get; set; }
    
        [Column(Name = "AuthorId")]
        public Guid AuthorId { get; set; }
    
        [Column(Name = "LastPostId")]
        public Guid LastPostId { get; set; }
    
        public string Title { get; set; }
    
        public string Description { get; set; }
    
        [Column(Name = "TopicCount")]
        public int TopicCount { get; set; }
    }
```
