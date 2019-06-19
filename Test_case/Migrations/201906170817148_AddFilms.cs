namespace Test_case.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFilms : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Films",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CreatorId = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 300),
                        Description = c.String(),
                        Year = c.Int(nullable: false),
                        Producer = c.String(maxLength: 300),
                        Path = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatorId)
                .Index(t => t.CreatorId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Films", "CreatorId", "dbo.AspNetUsers");
            DropIndex("dbo.Films", new[] { "CreatorId" });
            DropTable("dbo.Films");
        }
    }
}
