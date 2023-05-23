using FluentMigrator;

namespace IdentityApi.Migrations;

[Migration(202305231000)]
public class InitialMigration_202305231000 : Migration
{
	public override void Up()
	{
		const string sql = """
		if not exists (select 1 from sys.tables where name = 'User')
			create table [User] (
				id uniqueidentifier not null,
				username nvarchar(32) not null, 
				email nvarchar(254) not null,
				password nvarchar(256) not null,
				created_at datetime2 not null,
				updated_at datetime2 not null,
				role nvarchar(16) not null,
				
				constraint PK_User PRIMARY KEY (id),
				constraint UQ_User_username Unique (username),
				constraint UQ_User_email UNIQUE (email)
			)
		
		if not exists (select 1 from sys.tables where name = 'RefreshToken')
			create table [RefreshToken] (
				id uniqueidentifier not null,
				jti nvarchar(36) not null,
				created_at bigint not null,
				expires_at bigint not null,
				used bit not null,
				invalidated bit not null,
				user_id uniqueidentifier not null,
				
				constraint PK_RefreshToken primary key (id),
				constraint FK_RefreshToken_User foreign key (user_id) references dbo.[User]
			)
		""";
		
		Execute.Sql(sql);
	}

	public override void Down()
	{
		const string sql = """
			if exists (select 1 from sys.tables where name = 'RefreshToken')
				drop table [RefreshToken]
			
			if exists (select 1 from sys.tables where name = 'User')
				drop table [User]
		""";
		
		Execute.Sql(sql);
	}
}