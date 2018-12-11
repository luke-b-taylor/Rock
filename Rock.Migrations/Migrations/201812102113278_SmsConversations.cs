// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class SmsConversations : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateCommunicationResponseUp();
            AddCommunicationRecipientSentMessageUp();
            UpdateDefinedType_COMMUNICATION_SMS_FROM_Up();
            AddUpdateDefinedValuesFor_COMMUNICATION_SMS_FROM_Up();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CreateCommunicationResponseDown();
            AddCommunicationRecipientSentMessageDown();
            UpdateDefinedType_COMMUNICATION_SMS_FROM_Down();
            AddUpdateDefinedValuesFor_COMMUNICATION_SMS_FROM_Down();
        }

        private void CreateCommunicationResponseUp()
        {

            CreateTable(
                "dbo.CommunicationResponse",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    FromPersonAliasId = c.Int(),
                    ToPersonAliasId = c.Int(),
                    IsRead = c.Boolean( nullable: false ),
                    RelatedSmsFromDefinedValueId = c.Int(),
                    RelatedCommunicationId = c.Int(),
                    RelatedTransportId = c.Int( nullable: false ),
                    RelatedMediumId = c.Int( nullable: false ),
                    Response = c.String(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ToPersonAliasId )
                .ForeignKey( "dbo.Communication", t => t.RelatedCommunicationId )
                .ForeignKey( "dbo.EntityType", t => t.RelatedTransportId )
                .ForeignKey( "dbo.EntityType", t => t.RelatedMediumId )
                .Index( t => t.ToPersonAliasId )
                .Index( t => t.RelatedCommunicationId )
                .Index( t => t.RelatedTransportId )
                .Index( t => t.RelatedMediumId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );
        }

        private void CreateCommunicationResponseDown()
        {
            DropForeignKey( "dbo.CommunicationResponse", "RelatedMediumId", "dbo.EntityType" );
            DropForeignKey( "dbo.CommunicationResponse", "RelatedTransportId", "dbo.EntityType" );
            DropForeignKey( "dbo.CommunicationResponse", "RelatedCommunicationId", "dbo.Communication" );
            DropForeignKey( "dbo.CommunicationResponse", "ToPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CommunicationResponse", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CommunicationResponse", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.CommunicationResponse", new[] { "Guid" } );
            DropIndex( "dbo.CommunicationResponse", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.CommunicationResponse", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.CommunicationResponse", new[] { "RelatedMediumId" } );
            DropIndex( "dbo.CommunicationResponse", new[] { "RelatedTransportId" } );
            DropIndex( "dbo.CommunicationResponse", new[] { "RelatedCommunicationId" } );
            DropIndex( "dbo.CommunicationResponse", new[] { "ToPersonAliasId" } );
            DropTable( "dbo.CommunicationResponse" );
        }

        private void AddCommunicationRecipientSentMessageUp()
        {
            AddColumn( "dbo.CommunicationRecipient", "SentMessage", c => c.String() );
        }

        private void AddCommunicationRecipientSentMessageDown()
        {
            DropColumn( "dbo.CommunicationRecipient", "SentMessage" );
        }

        /// <summary>
        /// Updates the defined type communication SMS from up.
        /// Updates properties of known defined value with guid 611BDE1F-7405-4D16-8626-CCFEDB0E62BE.
        /// </summary>
        private void UpdateDefinedType_COMMUNICATION_SMS_FROM_Up()
        {
            RockMigrationHelper.AddDefinedType( 
                "Communication", 
                "SMS Phone Numbers",
                "SMS numbers that can be used to send text messages from. This is usually a phone number or short code that has been set up with your SMS provider.Providing a response recipient will send replies straight to the individual’s mobile phone. Leaving this field blank will enable responses to be processed from within Rock.", 
                "611BDE1F-7405-4D16-8626-CCFEDB0E62BE" );
        }

        private void UpdateDefinedType_COMMUNICATION_SMS_FROM_Down()
        {
            RockMigrationHelper.AddDefinedType(
                "Communication",
                "SMS From Values",
                "Values that can be used to send text messages from.  This is usually a phone number or short code that has been set up with your SMS provider.",
                "611BDE1F-7405-4D16-8626-CCFEDB0E62BE" );
        }

        private void AddUpdateDefinedValuesFor_COMMUNICATION_SMS_FROM_Up()
        {
            // Edit DT Attribute Reponse Recipient and make optional
            RockMigrationHelper.UpdateDefinedTypeAttribute( "611BDE1F-7405-4D16-8626-CCFEDB0E62BE",
                "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70",
                "Response Recipient",
                "ResponseRecipient",
                "The person who should receive responses to the SMS number. This person must have a phone number with SMS enabled or no response will be sent.",
                24,
                true,
                string.Empty,
                false,
                false,
                "E9E82709-5506-4339-8F6A-C2259329A71F" );


            // Add DT Attribute Enable Mobile Conversations and set to true
            RockMigrationHelper.AddDefinedTypeAttribute( "611BDE1F-7405-4D16-8626-CCFEDB0E62BE",
                "1EDAFDED-DFE6-4334-B019-6EECBA89E05A",
                "Enable Mobile Conversations",
                "EnableMobileConversations",
                "When enabled SMS conversations would be processed by sending messages to the Response Recipient’s mobile phone. Otherwise, the conversations would be handled using the SMS Conversations page.",
                1019,
                "True",
                "60E05E00-E1A3-46A2-A56D-FE208D91FE4F" );

            // Add DT Attribute Value for Enable Mobile Conversations Attribute and set to true
            RockMigrationHelper.AddDefinedValueAttributeValue( "611BDE1F-7405-4D16-8626-CCFEDB0E62BE", "60E05E00-E1A3-46A2-A56D-FE208D91FE4F", "1" );

            // Add DT Attribute LaunchWorkflowOnResponseReceived 
            RockMigrationHelper.AddDefinedTypeAttribute( "611BDE1F-7405-4D16-8626-CCFEDB0E62BE",
                "46A03F59-55D3-4ACE-ADD5-B4642225DD20",
                "Launch Workflow On Response Received",
                "LaunchWorkflowOnResponseReceived",
                "",
                1020,
                string.Empty,
                "49C7A5A3-D711-4E41-86E4-06408ED6C1BD" );
        }

        private void AddUpdateDefinedValuesFor_COMMUNICATION_SMS_FROM_Down()
        {
            // Edit DT Attribute Reponse Recipient and make optional
            RockMigrationHelper.UpdateDefinedTypeAttribute( "611BDE1F-7405-4D16-8626-CCFEDB0E62BE",
                "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70",
                "Response Recipient",
                "ResponseRecipient",
                "The person who should receive responses to the SMS number. This person must have a phone number with SMS enabled or no response will be sent.",
                24,
                true,
                string.Empty,
                false,
                true,
                "E9E82709-5506-4339-8F6A-C2259329A71F" );

            // Add DT Attribute Enable Mobile Conversations and set to true
            RockMigrationHelper.DeleteAttribute( "60E05E00-E1A3-46A2-A56D-FE208D91FE4F" );

            // Add DT Attribute LaunchWorkflowOnResponseReceived 
            RockMigrationHelper.DeleteAttribute( "49C7A5A3-D711-4E41-86E4-06408ED6C1BD" );

        }

    }
}
