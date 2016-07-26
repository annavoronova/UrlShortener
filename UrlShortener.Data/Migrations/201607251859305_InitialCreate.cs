namespace UrlShortener.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tbl_shorten_url",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        long_url = c.String(nullable: false, maxLength: 1000),
                        segment = c.String(nullable: false, maxLength: 20),
                        added = c.DateTime(nullable: false),
                        ip = c.String(nullable: false, maxLength: 50),
                        num_of_clicks = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.tbl_statistics",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        click_date = c.DateTime(nullable: false),
                        ip = c.String(nullable: false, maxLength: 50),
                        referrer = c.String(maxLength: 500),
                        shortUrl_id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.tbl_shorten_url", t => t.shortUrl_id, cascadeDelete: true)
                .Index(t => t.shortUrl_id);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.tbl_statistics", new[] { "shortUrl_id" });
            DropForeignKey("dbo.tbl_statistics", "shortUrl_id", "dbo.tbl_shorten_url");
            DropTable("dbo.tbl_statistics");
            DropTable("dbo.tbl_shorten_url");
        }
    }
}
