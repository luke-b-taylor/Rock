using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class CommunicationResponseService
    {
        /// <summary>
        /// Gets the responses sent from a person Alias ID without any other filters.
        /// </summary>
        /// <param name="fromPersonAliasId">From person alias identifier.</param>
        /// <returns></returns>
        public IQueryable GetResponsesFromPersonAliasId( int fromPersonAliasId )
        {
            return Queryable().Where( r => r.FromPersonAliasId == fromPersonAliasId );
        }

        /// <summary>
        /// Gets the responses from a person Alias ID for the SMS Phone number.
        /// </summary>
        /// <param name="fromPersonAliasId">From person alias identifier.</param>
        /// <param name="relatedSmsFromDefinedValueId">The related SMS from defined value identifier.</param>
        /// <returns></returns>
        public IQueryable GetResponsesFromPersonAliasIdForSMSNumber( int fromPersonAliasId, int relatedSmsFromDefinedValueId )
        {
            return Queryable()
                .Where( r => r.FromPersonAliasId == fromPersonAliasId )
                .Where( r => r.RelatedSmsFromDefinedValueId == relatedSmsFromDefinedValueId );
        }


        /// <summary>
        /// Gets the SMS conversation history for a person alias ID. Inclues the communication sent by Rock that the person may be responding to.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="relatedSmsFromDefinedValueId">The releated SMS from defined value identifier.</param>
        /// <returns></returns>
        public DataSet GetConversation( int personAliasId, int relatedSmsFromDefinedValueId )
        {
            var sqlParams = new Dictionary<string, object>();
            sqlParams.Add( "@personAliasId", personAliasId );
            sqlParams.Add( "@releatedSmsFromDefinedValueId", relatedSmsFromDefinedValueId );
            sqlParams.Add( "@smsMediumEntityTypeId", EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS ).Id );

            string sql = @"
                SELECT [Response], [CreatedDateTime], [FromPersonAliasId], [MessageKey]
                FROM [CommunicationResponse] cr
                WHERE ( cr.[FromPersonAliasId] = @personAliasId )
                    AND cr.[RelatedSmsFromDefinedValueId] = @releatedSmsFromDefinedValueId
                    AND cr.[RelatedMediumEntityTypeId] = @smsMediumEntityTypeId
                UNION
                SELECT c.[SMSMessage], c.[CreatedDateTime], c.[SenderPersonAliasId], ''
                FROM [Communication] c
                JOIN [CommunicationRecipient] rec ON c.[Id] = rec.[CommunicationId]
                WHERE rec.[PersonAliasId] = @personAliasId
                    AND c.[SMSFromDefinedValueId] = @releatedSmsFromDefinedValueId
                ORDER BY [CreatedDateTime]";

            var set = Rock.Data.DbService.GetDataSet( sql, CommandType.Text, sqlParams );

            return set;
        }

        /// <summary>
        /// Gets the conversation for a message key. Use this if a person was not able to be
        /// determined.
        /// </summary>
        /// <param name="messageKey">The message key.</param>
        /// <param name="relatedSmsFromDefinedValueId">The related SMS from defined value identifier.</param>
        /// <returns></returns>
        public DataSet GetConversation( string messageKey, int relatedSmsFromDefinedValueId )
        {
            var sqlParams = new Dictionary<string, object>();
            sqlParams.Add( "@messageKey", messageKey );
            sqlParams.Add( "@releatedSmsFromDefinedValueId", relatedSmsFromDefinedValueId );
            sqlParams.Add( "@smsMediumEntityTypeId", EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS ).Id );

            string sql = @"
                SELECT [Response], [CreatedDateTime], [FromPersonAliasId], [MessageKey]
                FROM [CommunicationResponse] cr
                WHERE ( cr.[MessageKey] = @messageKey )
                    AND cr.[RelatedSmsFromDefinedValueId] = @releatedSmsFromDefinedValueId
                    AND cr.[RelatedMediumEntityTypeId] = @smsMediumEntityTypeId
                ORDER BY [CreatedDateTime]";

            var set = Rock.Data.DbService.GetDataSet( sql, CommandType.Text, sqlParams );

            return set;
        }

        /// <summary>
        /// Updates the IsRead property of SMS Responses sent from the provided person to the SMSPhone number stored in SmsFromDefinedValue.
        /// </summary>
        /// <param name="fromPersonAliasId">From person alias identifier.</param>
        /// <param name="relatedSmsFromDefinedValueId">The defined value ID of the from SMS phone number.</param>
        public void UpdateReadPropertyByFromPersonAliasId( int fromPersonAliasId, int relatedSmsFromDefinedValueId )
        {
            var sqlParams = new Dictionary<string, object>();
            sqlParams.Add( "@fromPersonAliasId", fromPersonAliasId );
            sqlParams.Add( "@releatedSmsFromDefinedValueId", relatedSmsFromDefinedValueId );
            sqlParams.Add( "@smsMediumEntityTypeId", EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS ).Id );

            string sql = @"
                UPDATE [CommunicationResponse]
                SET IsRead = 1
                WHERE [FromPersonAliasId] = @fromPersonAliasId
                    AND [RelatedSmsFromDefinedValueId] = @releatedSmsFromDefinedValueId
                    AND [RelatedMediumEntityTypeId] = @smsMediumEntityTypeId
                    AND [IsRead] = 0";

            Rock.Data.DbService.ExecuteCommand( sql, CommandType.Text, sqlParams );
        }

        /// <summary>
        /// Updates the IsRead property of SMS Responses sent from the provided MessageKey to the SMSPhone number stored in SmsFromDefinedValue.
        /// The MessageKey is the transport address of the sender, e.g. an SMS enabled phone number or email address.
        /// </summary>
        /// <param name="messageKey">The message key.</param>
        /// <param name="relatedSmsFromDefinedValueId">The related SMS from defined value identifier.</param>
        public void UpdateReadPropertyByMessageKey( string messageKey, int relatedSmsFromDefinedValueId )
        {
            var sqlParams = new Dictionary<string, object>();
            sqlParams.Add( "@messageKey", messageKey );
            sqlParams.Add( "@releatedSmsFromDefinedValueId", relatedSmsFromDefinedValueId );
            sqlParams.Add( "@smsMediumEntityTypeId", EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS ).Id );

            string sql = @"
                UPDATE [CommunicationResponse]
                SET IsRead = 1
                WHERE [MessageKey] = @messageKey
                    AND [RelatedSmsFromDefinedValueId] = @releatedSmsFromDefinedValueId
                    AND [RelatedMediumEntityTypeId] = @smsMediumEntityTypeId
                    AND [IsRead] = 0";

            Rock.Data.DbService.ExecuteCommand( sql, CommandType.Text, sqlParams );
        }

        public void UpdatePersonAliasByMessageKey( int personAliasId, string messageKey, PersonAliasType personAliasType )
        {
            string sql = string.Empty;

            switch ( personAliasType )
            {
                case PersonAliasType.FromPersonAlias:
                    UpdateFromPersonAliasByMessageKey( personAliasId, messageKey );
                    break;
                case PersonAliasType.ToPersonAlias:
                    UpdateToPersonAliasByMessageKey( personAliasId, messageKey );
                    break;
                default:
                    break;
            }
        }

        private void UpdateToPersonAliasByMessageKey( int personAliasId, string messageKey )
        {
            var sqlParams = new Dictionary<string, object>();
            sqlParams.Add( "@personAliasId", personAliasId );
            sqlParams.Add( "@messageKey", messageKey );

            string sql = @"
                UPDATE [CommunicationResponse]
                SET [ToPersonAliasId] = @personAliasId
                WHERE [MessageKey] = @messageKey";

            Rock.Data.DbService.ExecuteCommand( sql, CommandType.Text, sqlParams );
        }

        private void UpdateFromPersonAliasByMessageKey( int personAliasId, string messageKey )
        {
            var sqlParams = new Dictionary<string, object>();
            sqlParams.Add( "personAliasId", personAliasId );
            sqlParams.Add( "messageKey", messageKey );

            string sql = @"
                UPDATE [CommunicationResponse]
                SET [FromPersonAliasId] = @personAliasId
                WHERE [MessageKey] = @messageKey";
            Rock.Data.DbService.ExecuteCommand( sql, CommandType.Text, sqlParams );
        }
    }

    public enum PersonAliasType
    {
        FromPersonAlias = 0,
        ToPersonAlias = 1
    }
}
