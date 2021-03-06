BEGIN
	DECLARE @CodeGen NVARCHAR(max) = ''

	SELECT @CodeGen += + '            // ' + se.Title + CHAR(13) + CHAR(10) + CONCAT (
			'            RockMigrationHelper.UpdateSystemEmail( '
			,'"'
			,c.[Name]
			,'", '
			,'"'
			,se.[Title]
			,'", '
			,'"'
			,se.[From]
			,'", '
			,'"'
			,se.[FromName]
			,'", '
			,'"'
			,se.[To]
			,'", '
			,'"'
			,se.[Cc]
			,'", '
			,'"'
			,se.[Bcc]
			,'", '
			,'"'
			,se.[Subject]
			,'", '
			,'@"' + Replace(se.[Body], '"', '""') + '", '
			,'"'
			,se.[Guid]
			,'");'
			) + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10)
	FROM [SystemEmail] se
	LEFT OUTER JOIN [Category] c ON se.CategoryId = c.Id
	WHERE (se.[IsSystem] = 0)
		-- exclude known system email templates that ship with IsSystem = 0
		AND se.[Guid] NOT IN (
			'158607D1-0772-4947-ADD6-EA31AB6ABC2F'
			,'91EA23C3-2E16-2597-4EAF-27C40D3A66D8'
			,'18521B26-1C7D-E287-487D-97D176CA4986'
			,'BC490DD4-ABBB-7DBA-4A9E-74F07F4B5881'
			,'7DBF229E-7DEE-A684-4929-6C37312A0039'
			)
	ORDER BY se.Id DESC

	SELECT 'MigrateSystemEmailsUp();
	' [Up]

	SELECT CONCAT (
			'
/// <summary>
        /// Migrates new system emails up.
        /// Code Generated from Dev Tools\Sql\CodeGen_SystemEmail.sql
        /// </summary>
        private void MigrateSystemEmailsUp()
        {
		'
			,@CodeGen
			,'}
		'
			) [MigrateSystemEmailsUp];

	SELECT '// ' + se.Title + CHAR(13) + CHAR(10) + Concat('RockMigrationHelper.DeleteSystemEmail("', se.[Guid], '");') [Down]
	FROM SystemEmail se
	WHERE (se.[IsSystem] = 0)
		-- exclude known system email templates that ship with IsSystem = 0
		AND se.[Guid] NOT IN (
			'158607D1-0772-4947-ADD6-EA31AB6ABC2F'
			,'91EA23C3-2E16-2597-4EAF-27C40D3A66D8'
			,'18521B26-1C7D-E287-487D-97D176CA4986'
			,'BC490DD4-ABBB-7DBA-4A9E-74F07F4B5881'
			,'7DBF229E-7DEE-A684-4929-6C37312A0039'
			)
	ORDER BY se.Id DESC
END
