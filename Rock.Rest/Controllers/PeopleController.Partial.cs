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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.OData;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    ///
    /// </summary>
    public partial class PeopleController
    {
        #region Get

        /// <summary>
        /// GET endpoint to get a single person record
        /// </summary>
        /// <param name="id">The Id of the record</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        [ActionName( "GetById" )]
        public override Person GetById( int id )
        {
            // NOTE: We want PrimaryAliasId to be populated, so call this.Get( true ) which includes "Aliases"
            var person = this.Get( true ).FirstOrDefault( a => a.Id == id );
            if ( person == null )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            return person;
        }

        /// <summary>
        /// GET endpoint to get a single person record
        /// </summary>
        /// <param name="key">The Id of the record</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [EnableQuery]
        public override Person Get( [FromODataUri] int key )
        {
            // NOTE: We want PrimaryAliasId to be populated, so call this.GetById( key ) which includes "Aliases"
            return this.GetById( key );
        }

        /// <summary>
        /// Queryable GET endpoint. Note that records that are marked as Deceased are not included
        /// </summary>
        /// <returns></returns>
        [Authenticate, Secured]
        [EnableQuery]
        public override IQueryable<Person> Get()
        {
            // NOTE: We want PrimaryAliasId to be populated, so include Aliases
            return base.Get().Include( a => a.Aliases );
        }

        /// <summary>
        /// Queryable GET endpoint with an option to include person records that have been marked as Deceased
        /// </summary>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [EnableQuery]
        public IQueryable<Person> Get( bool includeDeceased )
        {
            var rockContext = this.Service.Context as RockContext;

            // NOTE: We want PrimaryAliasId to be populated, so include "Aliases"
            return new PersonService( rockContext ).Queryable( includeDeceased ).Include( a => a.Aliases );
        }

        /// <summary>
        /// Gets the currently authenticated person
        /// </summary>
        /// <returns>A person</returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetCurrentPerson" )]
        public Person GetCurrentPerson()
        {
            var rockContext = new Rock.Data.RockContext();
            return new PersonService( rockContext ).Get( GetPerson().Id );
        }

        /// <summary>
        /// Searches the person records based on the specified email address
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetByEmail/{email}" )]
        public IQueryable<Person> GetByEmail( string email )
        {
            var rockContext = new Rock.Data.RockContext();
            return new PersonService( rockContext ).GetByEmail( email, true ).Include( a => a.Aliases );
        }

        /// <summary>
        /// Searches the person records based on the specified phone number. NOTE that partial matches are included
        /// </summary>
        /// <param name="number">The phone number.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetByPhoneNumber/{number}" )]
        public IQueryable<Person> GetByPhoneNumber( string number )
        {
            var rockContext = new Rock.Data.RockContext();
            return new PersonService( rockContext ).GetByPhonePartial( number, true ).Include( a => a.Aliases );
        }

        /// <summary>
        /// GET a person record based on the specified username
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetByUserName/{username}" )]
        public Person GetByUserName( string username )
        {
            int? personId = new UserLoginService( ( Rock.Data.RockContext ) Service.Context ).Queryable()
                .Where( u => u.UserName.Equals( username ) )
                .Select( a => a.PersonId )
                .FirstOrDefault();

            if ( personId != null )
            {
                return Service.Queryable().Include( a => a.PhoneNumbers ).Include( a => a.Aliases )
                    .FirstOrDefault( p => p.Id == personId.Value );
            }

            throw new HttpResponseException( System.Net.HttpStatusCode.NotFound );
        }

        /// <summary>
        /// Gets a list of people's names, email, gender and birthdate, to see if there are potential duplicates.
        /// For example, you might want to use this during account creation to warn that the person might already have an account.
        /// </summary>
        /// <param name="lastName">The last name.</param>
        /// <param name="emailAddress">The email address.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetPotentialDuplicates" )]
        public IEnumerable<DuplicatePersonInfo> GetPotentialDuplicates( string lastName, string emailAddress )
        {
            // return a limited number of fields so that this endpoint could be made available to a wider audience
            return Get().Where( a => a.Email == emailAddress && a.LastName == lastName ).ToList().Select( a => new DuplicatePersonInfo
            {
                Id = a.Id,
                Name = a.FullName,
                Email = a.Email,
                Gender = a.Gender,
                BirthDay = a.BirthDay,
                BirthMonth = a.BirthMonth,
                BirthYear = a.BirthYear
            } );
        }

        /// <summary>
        ///
        /// </summary>
        public class DuplicatePersonInfo
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the email.
            /// </summary>
            /// <value>
            /// The email.
            /// </value>
            public string Email { get; set; }

            /// <summary>
            /// Gets or sets the gender.
            /// </summary>
            /// <value>
            /// The gender.
            /// </value>
            public Gender Gender { get; set; }

            /// <summary>
            /// Gets or sets the birth month.
            /// </summary>
            /// <value>
            /// The birth month.
            /// </value>
            public int? BirthMonth { get; set; }

            /// <summary>
            /// Gets or sets the birth day.
            /// </summary>
            /// <value>
            /// The birth day.
            /// </value>
            public int? BirthDay { get; set; }

            /// <summary>
            /// Gets or sets the birth year.
            /// </summary>
            /// <value>
            /// The birth year.
            /// </value>
            public int? BirthYear { get; set; }
        }

        /// <summary>
        /// GET the Person by person alias identifier.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetByPersonAliasId/{personAliasId}" )]
        public Person GetByPersonAliasId( int personAliasId )
        {
            int? personId = new PersonAliasService( ( Rock.Data.RockContext ) Service.Context ).Queryable()
                .Where( u => u.Id.Equals( personAliasId ) ).Select( a => a.PersonId ).FirstOrDefault();
            if ( personId != null )
            {
                return this.Get( personId.Value );
            }

            throw new HttpResponseException( System.Net.HttpStatusCode.NotFound );
        }

        /// <summary>
        /// Gets the graduation year based on the provided GradeOffset
        /// </summary>
        /// <param name="gradeOffset">The grade offset for the person.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetGraduationYear/{gradeOffset}" )]
        public int GetGraduationYear( int gradeOffset )
        {
            int? graduationYear = Person.GraduationYearFromGradeOffset( gradeOffset );
            if ( graduationYear.HasValue )
            {
                return graduationYear.Value;
            }

            throw new HttpResponseException( System.Net.HttpStatusCode.NotFound );
        }

        #endregion

        #region Post

        /// <summary>
        /// Adds a new person and puts them into a new family
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override System.Net.Http.HttpResponseMessage Post( Person person )
        {
            SetProxyCreation( true );

            CheckCanEdit( person );

            if ( !person.IsValid )
            {
                return ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    string.Join( ",", person.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );
            }

            System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );
            PersonService.SaveNewPerson( person, ( Rock.Data.RockContext ) Service.Context, null, false );

            return ControllerContext.Request.CreateResponse( HttpStatusCode.Created, person.Id );
        }

        /// <summary>
        /// Adds a new person and adds them to the specified family.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="familyId">The family identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/People/AddNewPersonToFamily/{familyId}" )]
        public System.Net.Http.HttpResponseMessage AddNewPersonToFamily( Person person, int familyId, int groupRoleId )
        {
            SetProxyCreation( true );

            CheckCanEdit( person );

            if ( !person.IsValid )
            {
                return ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    string.Join( ",", person.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );
            }

            System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );

            PersonService.AddPersonToFamily( person, person.Id == 0, familyId, groupRoleId, ( Rock.Data.RockContext ) Service.Context );

            return ControllerContext.Request.CreateResponse( HttpStatusCode.Created, person.Id );
        }

        /// <summary>
        /// Adds the existing person to family, optionally removing them from any other families they belong to
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="familyId">The family identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <param name="removeFromOtherFamilies">if set to <c>true</c> [remove from other families].</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/People/AddExistingPersonToFamily" )]
        public System.Net.Http.HttpResponseMessage AddExistingPersonToFamily( int personId, int familyId, int groupRoleId, bool removeFromOtherFamilies )
        {
            SetProxyCreation( true );

            System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );
            var person = this.Get( personId );
            CheckCanEdit( person );

            PersonService.AddPersonToFamily( person, false, familyId, groupRoleId, ( Rock.Data.RockContext ) Service.Context );

            if ( removeFromOtherFamilies )
            {
                PersonService.RemovePersonFromOtherFamilies( familyId, personId, ( Rock.Data.RockContext ) Service.Context );
            }

            return ControllerContext.Request.CreateResponse( HttpStatusCode.Created, person.Id );
        }

        #endregion

        #region Search

        /// <summary>
        /// Returns results to the Person Picker
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="includeHtml">if set to <c>true</c> [include HTML].</param>
        /// <param name="includeDetails">if set to <c>true</c> [include details].</param>
        /// <param name="includeBusinesses">if set to <c>true</c> [include businesses].</param>
        /// <param name="includeDeceased">if set to <c>true</c> [include deceased].</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/Search" )]
        public IQueryable<PersonSearchResult> Search( string name, bool includeHtml, bool includeDetails, bool includeBusinesses = false, bool includeDeceased = false )
        {
            int count = GlobalAttributesCache.Value( "core.PersonPickerFetchCount" ).AsIntegerOrNull() ?? 60;
            bool showFullNameReversed;
            bool allowFirstNameOnly = false;

            var searchComponent = Rock.Search.SearchContainer.GetComponent( typeof( Rock.Search.Person.Name ).FullName );
            if ( searchComponent != null )
            {
                allowFirstNameOnly = searchComponent.GetAttributeValue( "FirstNameSearch" ).AsBoolean();
            }

            var activeRecordStatusValue = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
            int activeRecordStatusValueId = activeRecordStatusValue != null ? activeRecordStatusValue.Id : 0;

            IQueryable<Person> sortedPersonQry = ( this.Service as PersonService )
                .GetByFullNameOrdered( name, true, includeBusinesses, allowFirstNameOnly, out showFullNameReversed ).Take( count );

            if ( includeDetails == false )
            {
                var personService = this.Service as PersonService;

                var simpleResult = sortedPersonQry.AsNoTracking().ToList().Select( a => new PersonSearchResult
                {
                    Id = a.Id,
                    Name = showFullNameReversed
                    ? Person.FormatFullNameReversed( a.LastName, a.NickName, a.SuffixValueId, a.RecordTypeValueId )
                    : Person.FormatFullName( a.NickName, a.LastName, a.SuffixValueId, a.RecordTypeValueId ),
                    IsActive = a.RecordStatusValueId.HasValue && a.RecordStatusValueId == activeRecordStatusValueId,
                    RecordStatus = a.RecordStatusValueId.HasValue ? DefinedValueCache.Get( a.RecordStatusValueId.Value ).Value : string.Empty,
                    Age = Person.GetAge( a.BirthDate ) ?? -1,
                    FormattedAge = a.FormatAge(),
                    SpouseName = personService.GetSpouse( a, x => x.Person.NickName )
                } );

                return simpleResult.AsQueryable();
            }
            else
            {
                List<PersonSearchResult> searchResult = SearchWithDetails( sortedPersonQry, showFullNameReversed );
                return searchResult.AsQueryable();
            }
        }

        /// <summary>
        /// Gets the search details (for the person picker)
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetSearchDetails" )]
        public string GetSearchDetails( int id )
        {
            PersonSearchResult personSearchResult = new PersonSearchResult();
            var person = this.Get().Include( a => a.PhoneNumbers ).Where( a => a.Id == id ).FirstOrDefault();
            if ( person != null )
            {
                GetPersonSearchDetails( personSearchResult, person );
                // Generate the HTML for the ConnectionStatus; "label-success" matches the default config of the
                // connection status badge on the Bio bar, but I think label-default works better here.
                string connectionStatusHtml = string.IsNullOrWhiteSpace( personSearchResult.ConnectionStatus ) ? string.Empty : string.Format( "<span class='label label-default pull-right'>{0}</span>", personSearchResult.ConnectionStatus );
                string searchDetailsFormat = @"{0}{1}<div class='contents'>{2}</div>";
                return string.Format( searchDetailsFormat, personSearchResult.PickerItemDetailsImageHtml, connectionStatusHtml, personSearchResult.PickerItemDetailsPersonInfoHtml );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a List of PersonSearchRecord based on the sorted person query
        /// </summary>
        /// <param name="sortedPersonQry">The sorted person qry.</param>
        /// <param name="showFullNameReversed">if set to <c>true</c> [show full name reversed].</param>
        /// <returns></returns>
        private List<PersonSearchResult> SearchWithDetails( IQueryable<Person> sortedPersonQry, bool showFullNameReversed )
        {
            var rockContext = this.Service.Context as Rock.Data.RockContext;
            var phoneNumbersQry = new PhoneNumberService( rockContext ).Queryable();
            var sortedPersonList = sortedPersonQry.Include( a => a.PhoneNumbers ).AsNoTracking().ToList();
            Guid activeRecord = new Guid( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE );

            List<PersonSearchResult> searchResult = new List<PersonSearchResult>();
            foreach ( var person in sortedPersonList )
            {
                PersonSearchResult personSearchResult = new PersonSearchResult();
                personSearchResult.Id = person.Id;
                personSearchResult.Name = showFullNameReversed ? person.FullNameReversed : person.FullName;
                if ( person.RecordStatusValueId.HasValue )
                {
                    var recordStatus = DefinedValueCache.Get( person.RecordStatusValueId.Value );
                    personSearchResult.RecordStatus = recordStatus.Value;
                    personSearchResult.IsActive = recordStatus.Guid.Equals( activeRecord );
                }
                else
                {
                    personSearchResult.RecordStatus = string.Empty;
                    personSearchResult.IsActive = false;
                }

                GetPersonSearchDetails( personSearchResult, person );

                searchResult.Add( personSearchResult );
            }

            return searchResult;
        }

        /// <summary>
        /// Gets the person search details.
        /// </summary>
        /// <param name="personSearchResult">The person search result.</param>
        /// <param name="person">The person.</param>
        private void GetPersonSearchDetails( PersonSearchResult personSearchResult, Person person )
        {
            var rockContext = this.Service.Context as Rock.Data.RockContext;

            var appPath = System.Web.VirtualPathUtility.ToAbsolute( "~" );
            string itemDetailFormat = @"
<div class='picker-select-item-details clearfix' style='display: none;'>
	{0}
	<div class='contents'>
        {1}
	</div>
</div>
";

            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            int adultRoleId = familyGroupType.Roles.First( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;

            int groupTypeFamilyId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ).Id;

            // figure out Family, Address, Spouse
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );

            Guid? recordTypeValueGuid = null;
            if ( person.RecordTypeValueId.HasValue )
            {
                recordTypeValueGuid = DefinedValueCache.Get( person.RecordTypeValueId.Value ).Guid;
            }

            personSearchResult.ImageHtmlTag = Person.GetPersonPhotoImageTag( person, 50, 50 );
            personSearchResult.Age = person.Age.HasValue ? person.Age.Value : -1;
            personSearchResult.ConnectionStatus = person.ConnectionStatusValueId.HasValue ? DefinedValueCache.Get( person.ConnectionStatusValueId.Value ).Value : string.Empty;
            personSearchResult.Gender = person.Gender.ConvertToString();
            personSearchResult.Email = person.Email;

            string imageHtml = string.Format(
                "<div class='person-image' style='background-image:url({0}&width=65);background-size:cover;background-position:50%'></div>",
                Person.GetPersonPhotoUrl( person, 200, 200 ) );

            string personInfoHtml = string.Empty;
            Guid matchLocationGuid;
            bool isBusiness;
            if ( recordTypeValueGuid.HasValue && recordTypeValueGuid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() )
            {
                isBusiness = true;
                matchLocationGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid();
            }
            else
            {
                isBusiness = false;
                matchLocationGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid();
            }

            var familyGroupMember = groupMemberService.Queryable()
                .Where( a => a.PersonId == person.Id )
                .Where( a => a.Group.GroupTypeId == groupTypeFamilyId )
                .OrderBy( a => a.GroupOrder ?? int.MaxValue )
                .Select( s => new
                {
                    s.GroupRoleId,
                    GroupLocation = s.Group.GroupLocations.Where( a => a.GroupLocationTypeValue.Guid == matchLocationGuid ).Select( a => a.Location ).FirstOrDefault()
                } ).FirstOrDefault();

            int? personAge = person.Age;

            if ( familyGroupMember != null )
            {
                if ( isBusiness )
                {
                    personInfoHtml += "Business";
                }
                else
                {
                    personInfoHtml += "<span class='role'>" + familyGroupType.Roles.First( a => a.Id == familyGroupMember.GroupRoleId ).Name + "</span>";
                }

                if ( personAge != null )
                {
                    personInfoHtml += " <em class='age'>(" + person.FormatAge() + " old)</em>";
                }

                if ( familyGroupMember.GroupRoleId == adultRoleId )
                {
                    var personService = this.Service as PersonService;
                    var spouse = personService.GetSpouse( person, a => new
                    {
                        a.Person.NickName,
                        a.Person.LastName,
                        a.Person.SuffixValueId
                    } );

                    if ( spouse != null )
                    {
                        string spouseFullName = Person.FormatFullName( spouse.NickName, spouse.LastName, spouse.SuffixValueId );
                        personInfoHtml += "<p class='spouse'><strong>Spouse:</strong> " + spouseFullName + "</p>";
                        personSearchResult.SpouseName = spouseFullName;
                    }
                }
            }
            else
            {
                if ( personAge != null )
                {
                    personInfoHtml += personAge.ToString() + " yrs old";
                }
            }

            if ( familyGroupMember != null )
            {
                var location = familyGroupMember.GroupLocation;

                if ( location != null )
                {
                    string addressHtml = "<dl class='address'><dt>Address</dt><dd>" + location.GetFullStreetAddress().ConvertCrLfToHtmlBr() + "</dd></dl>";
                    personSearchResult.Address = location.GetFullStreetAddress();
                    personInfoHtml += addressHtml;
                }
            }

            // Generate the HTML for Email and PhoneNumbers
            if ( !string.IsNullOrWhiteSpace( person.Email ) || person.PhoneNumbers.Any() )
            {
                string emailAndPhoneHtml = "<div class='margin-t-sm'>";
                emailAndPhoneHtml += "<span class='email'>" + person.Email + "</span>";
                string phoneNumberList = "<ul class='phones list-unstyled'>";
                foreach ( var phoneNumber in person.PhoneNumbers )
                {
                    var phoneType = DefinedValueCache.Get( phoneNumber.NumberTypeValueId ?? 0 );
                    phoneNumberList += string.Format(
                        "<li>{0} <small>{1}</small></li>",
                        phoneNumber.IsUnlisted ? "Unlisted" : phoneNumber.NumberFormatted,
                        phoneType != null ? phoneType.Value : string.Empty );
                }

                emailAndPhoneHtml += phoneNumberList + "</ul></div>";

                personInfoHtml += emailAndPhoneHtml;
            }

            // force the link to open a new scrollable,resizable browser window (and make it work in FF, Chrome and IE) http://stackoverflow.com/a/2315916/1755417
            personInfoHtml += $"<p class='margin-t-sm'><small><a class='cursor-pointer' onclick=\"javascript: window.open('/person/{person.Id}', '_blank', 'scrollbars=1,resizable=1,toolbar=1'); return false;\" data-toggle=\"tooltip\" title=\"View Profile\">View Profile</a></small></p>";

            personSearchResult.PickerItemDetailsImageHtml = imageHtml;
            personSearchResult.PickerItemDetailsPersonInfoHtml = personInfoHtml;
            personSearchResult.PickerItemDetailsHtml = string.Format( itemDetailFormat, imageHtml, personInfoHtml );
        }

        /// <summary>
        /// Obsolete: Gets the search details
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetSearchDetails/{personId}" )]
        [Obsolete( "Returns incorrect results, will be removed in a future version" )]
        public string GetImpersonationParameterObsolete( int personId )
        {
            // NOTE: This route is called GetSearchDetails but really returns an ImpersonationParameter due to a copy/paste bug.
            // Marked obsolete but kept around in case anybody was taking advantage of this bug

            string result = string.Empty;

            var rockContext = this.Service.Context as Rock.Data.RockContext;

            var person = new PersonService( rockContext ).Queryable().Include( a => a.Aliases ).AsNoTracking().FirstOrDefault( a => a.Id == personId );

            if ( person != null )
            {
                result = person.ImpersonationParameter;
            }

            return result;
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the specified ExpireDateTime, UsageLimit, and Page
        /// Returns the encrypted URLEncoded Token along with the ImpersonationParameter key in the form of "rckipid={ImpersonationParameter}"
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="expireDateTime">The expire date time.</param>
        /// <param name="usageLimit">The usage limit.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/GetImpersonationParameter" )]
        public string GetImpersonationParameter( int personId, DateTime? expireDateTime = null, int? usageLimit = null, int? pageId = null )
        {
            string result = string.Empty;

            var rockContext = this.Service.Context as Rock.Data.RockContext;

            var person = new PersonService( rockContext ).Queryable().Include( a => a.Aliases ).AsNoTracking().FirstOrDefault( a => a.Id == personId );

            if ( person != null )
            {
                return person.GetImpersonationParameter( expireDateTime, usageLimit, pageId );
            }
            else
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }
        }

        /// <summary>
        /// Gets the popup html for the selected person
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/PopupHtml/{personId}" )]
        public PersonSearchResult GetPopupHtml( int personId )
        {
            return GetPopupHtml( personId, true );
        }

        /// <summary>
        /// Gets the popup html for the selected person
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <param name="emailAsLink">Determines if the email address should be formatted as a link.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/PopupHtml/{personId}/{emailAsLink}" )]
        public PersonSearchResult GetPopupHtml( int personId, bool emailAsLink )
        {
            var result = new PersonSearchResult();
            result.Id = personId;
            result.PickerItemDetailsHtml = "No Details Available";

            var html = new StringBuilder();

            // Create new service (need ProxyServiceEnabled)
            var rockContext = new Rock.Data.RockContext();
            var person = new PersonService( rockContext ).Queryable( "ConnectionStatusValue, PhoneNumbers" )
                .Where( p => p.Id == personId )
                .FirstOrDefault();

            if ( person != null )
            {
                Guid? recordTypeValueGuid = null;
                if ( person.RecordTypeValueId.HasValue )
                {
                    recordTypeValueGuid = DefinedValueCache.Get( person.RecordTypeValueId.Value ).Guid;
                }

                var appPath = System.Web.VirtualPathUtility.ToAbsolute( "~" );
                html.AppendFormat(
                    "<header>{0} <h3>{1}<small>{2}</small></h3></header>",
                    Person.GetPersonPhotoImageTag( person, 65, 65 ),
                    person.FullName,
                    person.ConnectionStatusValue != null ? person.ConnectionStatusValue.Value : string.Empty );

                html.Append( "<div class='body'>" );

                var spouse = person.GetSpouse( rockContext );
                if ( spouse != null )
                {
                    html.AppendFormat(
                        "<div><strong>Spouse</strong> {0}</div>",
                        spouse.LastName == person.LastName ? spouse.FirstName : spouse.FullName );
                }

                int? age = person.Age;
                if ( age.HasValue )
                {
                    html.AppendFormat( "<div><strong>Age</strong> {0}</div>", age );
                }

                if ( !string.IsNullOrWhiteSpace( person.Email ) )
                {
                    if ( emailAsLink )
                    {
                        html.AppendFormat( "<div style='text-overflow: ellipsis; white-space: nowrap; overflow:hidden; width: 245px;'><strong>Email</strong> {0}</div>", person.GetEmailTag( VirtualPathUtility.ToAbsolute( "~/" ) ) );
                    }
                    else
                    {
                        html.AppendFormat( "<div style='text-overflow: ellipsis; white-space: nowrap; overflow:hidden; width: 245px;'><strong>Email</strong> {0}</div>", person.Email );
                    }
                }

                foreach ( var phoneNumber in person.PhoneNumbers.Where( n => n.IsUnlisted == false && n.NumberTypeValueId.HasValue ).OrderBy( n => n.NumberTypeValue.Order ) )
                {
                    html.AppendFormat( "<div><strong>{0}</strong> {1}</div>", phoneNumber.NumberTypeValue.Value, phoneNumber.ToString() );
                }

                html.Append( "</div>" );

                result.PickerItemDetailsHtml = html.ToString();
            }

            return result;
        }

        #endregion

        #region Delete Override

        /// <summary>
        /// DELETE endpoint for a Person Record. NOTE: Person records can not be deleted using REST, so this will always return a 405
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="HttpResponseException"></exception>
        public override void Delete( int id )
        {
            // we don't want to support DELETE on a Person in ROCK (especially from REST).  So, return a MethodNotAllowed.
            throw new HttpResponseException( System.Net.HttpStatusCode.MethodNotAllowed );
        }

        #endregion

        #region Export

        /// <summary>
        /// Exports Person Records
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="sortBy">The sort by.</param>
        /// <param name="sortDirection">The sort direction.</param>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <param name="modifiedSince">The modified since.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/People/Export" )]
        public PeopleExport Export( int page, int pageSize, string sortBy = null, int sortDirection = 0, int? dataViewId = null, DateTime? modifiedSince = null )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            IQueryable<Person> personQry;
            SortProperty sortProperty;
            if ( sortBy.IsNotNullOrWhiteSpace() )
            {
                sortProperty = new SortProperty { Direction = ( System.Web.UI.WebControls.SortDirection ) sortDirection, Property = sortBy };
            }
            else
            {
                sortProperty = new SortProperty { Direction = ( System.Web.UI.WebControls.SortDirection ) sortDirection, Property = "Id" };
            }

            if ( dataViewId.HasValue )
            {
                var dataView = new DataViewService( rockContext ).GetNoTracking( dataViewId.Value );
                if ( dataView != null )
                {
                    List<string> errorMessages = null;
                    personQry = dataView.GetQuery( sortProperty, rockContext, null, out errorMessages ) as IQueryable<Person>;
                    if ( personQry == null )
                    {
                        throw new HttpResponseException( System.Net.HttpStatusCode.BadRequest ) { Source = $"DataViewId: {dataViewId}" };
                    }
                }
                else
                {
                    throw new HttpResponseException( System.Net.HttpStatusCode.NotFound ) { Source = $"DataViewId: {dataViewId}" };
                }
            }
            else
            {
                personQry = personService.Queryable();
                if ( sortProperty != null )
                {
                    personQry = personQry.Sort( sortProperty );
                }
            }

            // todo: limit to global attribute
            var fetchCount = pageSize;

            var skip = ( page - 1 ) * fetchCount;

            PeopleExport peopleExport = new PeopleExport();
            peopleExport.Page = page;
            peopleExport.PageSize = pageSize;
            peopleExport.TotalCount = personQry.Count();

            var pagedPersonQry = personQry
                .Include( a => a.PhoneNumbers )
                .AsNoTracking()
                .Skip( skip )
                .Take( fetchCount );

            Stopwatch stopwatch = Stopwatch.StartNew();
            var personList = pagedPersonQry
                .ToList();

            var toListMS = stopwatch.Elapsed.TotalMilliseconds;
            stopwatch.Restart();

            var familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;

            Guid homeAddressGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid();

            int homeAddressDefinedValueId = DefinedValueCache.GetId( homeAddressGuid ).Value;

            Dictionary<int, Location> personIdHomeLocationsLookup = new GroupMemberService( rockContext ).AsNoFilter()
                .Where( m => m.Group.GroupTypeId == familyGroupTypeId && pagedPersonQry.Any( p => p.Id == m.PersonId ) )
                .OrderBy( a => a.PersonId )
                .Select( m => new
                {
                    m.PersonId,
                    GroupOrder = m.GroupOrder ?? int.MaxValue,
                    Location = m.Group.GroupLocations.Where( a => a.GroupLocationTypeValueId == homeAddressDefinedValueId && a.IsMailingLocation ).Select( a => a.Location ).FirstOrDefault()
                } )
                .AsNoTracking()
                .ToList()
                .GroupBy( a => a.PersonId )
                .Select( a => new
                {
                    PersonId = a.Key,
                    Location = a.OrderBy( v => v.GroupOrder ).Select( s => s.Location ).FirstOrDefault()
                } )
                .ToDictionary( k => k.PersonId, v => v.Location );

            var personHomeLocationsLookupMS = stopwatch.Elapsed.TotalMilliseconds;
            stopwatch.Restart();

            peopleExport.Persons = personList.Select( p => new PersonExport( p, personIdHomeLocationsLookup ) ).ToList();
            var personExportsMS = stopwatch.Elapsed.TotalMilliseconds;
            stopwatch.Restart();

            var json = peopleExport.ToJson();
            var toJSONMS = stopwatch.Elapsed.TotalMilliseconds;

            Debug.WriteLine( $"{toListMS}ms, {personHomeLocationsLookupMS}ms, {personExportsMS}ms, {toJSONMS}ms" );

            return peopleExport;
        }
        #endregion
    }

    /// <summary>
    ///
    /// </summary>
    public class PersonSearchResult
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the full name last first.
        /// </summary>
        /// <value>
        /// The full name last first.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the image HTML tag.
        /// </summary>
        /// <value>
        /// The image HTML tag.
        /// </value>
        public string ImageHtmlTag { get; set; }

        /// <summary>
        /// Gets or sets the age in years
        /// NOTE: returns -1 if age is unknown
        /// </summary>
        /// <value>The age.</value>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the formatted age.
        /// </summary>
        /// <value>
        /// The formatted age.
        /// </value>
        public string FormattedAge { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        /// <value>The gender.</value>
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets the connection status.
        /// </summary>
        /// <value>The connection status.</value>
        public string ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the record status.
        /// </summary>
        /// <value>The member status.</value>
        public string RecordStatus { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the name of the spouse.
        /// </summary>
        /// <value>
        /// The name of the spouse.
        /// </value>
        public string SpouseName { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the picker item details HTML.
        /// </summary>
        /// <value>
        /// The picker item details HTML.
        /// </value>
        public string PickerItemDetailsHtml { get; set; }

        /// <summary>
        /// Gets or sets the picker item details image HTML.
        /// </summary>
        /// <value>
        /// The picker item details image HTML.
        /// </value>
        public string PickerItemDetailsImageHtml { get; set; }

        /// <summary>
        /// Gets or sets the picker item details person information HTML.
        /// </summary>
        /// <value>
        /// The picker item details person information HTML.
        /// </value>
        public string PickerItemDetailsPersonInfoHtml { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [RockClientInclude( "Export record from ~/api/People/Export" )]
    public class PeopleExport
    {
        /// <summary>
        /// Gets or sets the page (1 based) that is included in this export
        /// </summary>
        /// <value>
        /// The page.
        /// </value>
        public int Page { get; set; }

        /// <summary>
        /// The PageSize that was specified
        /// </summary>
        /// <value>
        /// The size of the page.
        /// </value>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of records (all pages)
        /// </summary>
        /// <value>
        /// The total count.
        /// </value>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the list persons in this page of the PeopleExport
        /// </summary>
        /// <value>
        /// The persons.
        /// </value>
        public List<PersonExport> Persons { get; set; }
    }

    /// <summary>
    /// Export record from ~/api/People/Export
    /// </summary>
    [RockClientInclude( "Export of Person record from ~/api/People/Export" )]
    public class PersonExport
    {

        /// <summary>
        /// The person
        /// </summary>
        private Person _person;


        /// <summary>
        /// Initializes a new instance of the <see cref="PersonExport" /> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="personIdHomeLocationsLookup">The person identifier home locations lookup.</param>
        public PersonExport( Person person, Dictionary<int, Location> personIdHomeLocationsLookup )
        {
            _person = person;
            this.HomeAddress = new LocationExport( personIdHomeLocationsLookup.GetValueOrNull( person.Id ) );
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id => _person.Id;

        /// <summary>
        /// The Person's salutation title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title
        {
            get
            {
                if ( _person.TitleValueId.HasValue )
                {
                    return DefinedValueCache.GetValue( _person.TitleValueId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        public string FirstName => _person.FirstName;

        /// <summary>
        /// Gets the name of the nick.
        /// </summary>
        /// <value>
        /// The name of the nick.
        /// </value>
        public string NickName => _person.NickName;

        /// <summary>
        /// Gets the name of the middle.
        /// </summary>
        /// <value>
        /// The name of the middle.
        /// </value>
        public string MiddleName => _person.MiddleName;

        /// <summary>
        /// Gets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName => _person.LastName;

        /// <summary>
        /// Gets the suffix.
        /// </summary>
        /// <value>
        /// The suffix.
        /// </value>
        public string Suffix
        {
            get
            {
                if ( _person.SuffixValueId.HasValue )
                {
                    return DefinedValueCache.GetValue( _person.SuffixValueId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the photo URL.
        /// </summary>
        /// <value>
        /// The photo URL.
        /// </value>
        public string PhotoUrl => _person.PhotoUrl;

        /// <summary>
        /// Gets the birth day.
        /// </summary>
        /// <value>
        /// The birth day.
        /// </value>
        public int? BirthDay => _person.BirthDay;

        /// <summary>
        /// Gets the birth month.
        /// </summary>
        /// <value>
        /// The birth month.
        /// </value>
        public int? BirthMonth => _person.BirthMonth;

        /// <summary>
        /// Gets the birth year.
        /// </summary>
        /// <value>
        /// The birth year.
        /// </value>
        public int? BirthYear => _person.BirthYear;

        /// <summary>
        /// Gets the gender.
        /// </summary>
        /// <value>
        /// The gender.
        /// </value>
        public string Gender => _person.Gender.ConvertToString();

        /// <summary>
        /// Gets the marital status value identifier.
        /// </summary>
        /// <value>
        /// The marital status value identifier.
        /// </value>
        public int? MaritalStatusValueId => _person.MaritalStatusValueId;

        /// <summary>
        /// Gets the marital status.
        /// </summary>
        /// <value>
        /// The marital status.
        /// </value>
        public string MaritalStatus
        {
            get
            {
                if ( _person.MaritalStatusValueId.HasValue )
                {
                    return DefinedValueCache.GetValue( _person.MaritalStatusValueId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the anniversary date.
        /// </summary>
        /// <value>
        /// The anniversary date.
        /// </value>
        public DateTime? AnniversaryDate => _person.AnniversaryDate;

        /// <summary>
        /// Gets the graduation year.
        /// </summary>
        /// <value>
        /// The graduation year.
        /// </value>
        public int? GraduationYear => _person.GraduationYear;

        /// <summary>
        /// Gets the giving group identifier.
        /// </summary>
        /// <value>
        /// The giving group identifier.
        /// </value>
        public int? GivingGroupId => _person.GivingGroupId;

        /// <summary>
        /// Gets the giving identifier.
        /// </summary>
        /// <value>
        /// The giving identifier.
        /// </value>
        public string GivingId => _person.GivingId;

        /// <summary>
        /// Gets the giving leader identifier.
        /// </summary>
        /// <value>
        /// The giving leader identifier.
        /// </value>
        public int GivingLeaderId => _person.GivingLeaderId;

        /// <summary>
        /// Gets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email => _person.Email;

        /// <summary>
        /// Gets the age classification.
        /// </summary>
        /// <value>
        /// The age classification.
        /// </value>
        public string AgeClassification => _person.AgeClassification.ConvertToString();

        /// <summary>
        /// Gets the primary family identifier.
        /// </summary>
        /// <value>
        /// The primary family identifier.
        /// </value>
        public int? PrimaryFamilyId => _person.PrimaryFamilyId;

        /// <summary>
        /// Gets the deceased date.
        /// </summary>
        /// <value>
        /// The deceased date.
        /// </value>
        public DateTime? DeceasedDate => _person.DeceasedDate;

        /// <summary>
        /// Gets a value indicating whether this instance is business.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is business; otherwise, <c>false</c>.
        /// </value>
        public bool IsBusiness => _person.IsBusiness();

        /// <summary>
        /// Gets the home phone.
        /// </summary>
        /// <value>
        /// The home phone.
        /// </value>
        public string HomePhone
        {
            get
            {
                return _person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() )?.Number;
            }
        }

        /// <summary>
        /// Gets the mobile phone.
        /// </summary>
        /// <value>
        /// The mobile phone.
        /// </value>
        public string MobilePhone
        {
            get
            {
                return _person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() )?.Number;
            }
        }

        /// <summary>
        /// Gets the home address.
        /// </summary>
        /// <value>
        /// The home address.
        /// </value>
        public LocationExport HomeAddress { get; private set; }

        /// <summary>
        /// Gets the record type value identifier.
        /// </summary>
        /// <value>
        /// The record type value identifier.
        /// </value>
        public int? RecordTypeValueId => _person.RecordTypeValueId;

        /// <summary>
        /// Gets the type of the record.
        /// </summary>
        /// <value>
        /// The type of the record.
        /// </value>
        public string RecordType
        {
            get
            {
                if ( _person.RecordTypeValueId.HasValue )
                {
                    return DefinedValueCache.GetValue( _person.RecordTypeValueId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the record status value identifier.
        /// </summary>
        /// <value>
        /// The record status value identifier.
        /// </value>
        public int? RecordStatusValueId => _person.RecordStatusValueId;

        /// <summary>
        /// Gets the record status.
        /// </summary>
        /// <value>
        /// The record status.
        /// </value>
        public string RecordStatus
        {
            get
            {
                if ( _person.RecordStatusValueId.HasValue )
                {
                    return DefinedValueCache.GetValue( _person.RecordStatusValueId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the record status last modified date time.
        /// </summary>
        /// <value>
        /// The record status last modified date time.
        /// </value>
        public DateTime? RecordStatusLastModifiedDateTime => _person.RecordStatusLastModifiedDateTime;

        /// <summary>
        /// Gets the record status reason value identifier.
        /// </summary>
        /// <value>
        /// The record status reason value identifier.
        /// </value>
        public int? RecordStatusReasonValueId => _person.RecordStatusReasonValueId;

        /// <summary>
        /// Gets the record status reason.
        /// </summary>
        /// <value>
        /// The record status reason.
        /// </value>
        public string RecordStatusReason
        {
            get
            {
                if ( _person.RecordStatusReasonValueId.HasValue )
                {
                    return DefinedValueCache.GetValue( _person.RecordStatusReasonValueId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the connection status value identifier.
        /// </summary>
        /// <value>
        /// The connection status value identifier.
        /// </value>
        public int? ConnectionStatusValueId => _person.ConnectionStatusValueId;

        /// <summary>
        /// Gets the connection status.
        /// </summary>
        /// <value>
        /// The connection status.
        /// </value>
        public string ConnectionStatus
        {
            get
            {
                if ( _person.ConnectionStatusValueId.HasValue )
                {
                    return DefinedValueCache.GetValue( _person.ConnectionStatusValueId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is deceased.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is deceased; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeceased => _person.IsDeceased;

        /// <summary>
        /// Gets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        public DateTime? CreatedDateTime => _person.CreatedDateTime;

        /// <summary>
        /// Gets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        public DateTime? ModifiedDateTime => _person.ModifiedDateTime;

        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid => _person.Guid;

        /// <summary>
        /// Gets the foreign key.
        /// </summary>
        /// <value>
        /// The foreign key.
        /// </value>
        public string ForeignKey => _person.ForeignKey;

        /// <summary>
        /// Gets the foreign identifier.
        /// </summary>
        /// <value>
        /// The foreign identifier.
        /// </value>
        public int? ForeignId => _person.ForeignId;

        /// <summary>
        /// Gets the foreign unique identifier.
        /// </summary>
        /// <value>
        /// The foreign unique identifier.
        /// </value>
        public Guid? ForeignGuid => _person.ForeignGuid;
    }

    /// <summary>
    /// 
    /// </summary>
    [RockClientInclude( "Export class for Addresses from ~/api/People/Export" )]
    public class LocationExport
    {
        private Location _location;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationExport" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        public LocationExport( Location location )
        {
            _location = location;
        }

        /// <summary>
        /// Gets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        public double? Latitude => _location?.Latitude;

        /// <summary>
        /// Gets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        public double? Longitude => _location?.Longitude;

        /// <summary>
        /// Gets the street1.
        /// </summary>
        /// <value>
        /// The street1.
        /// </value>
        public string Street1 => _location?.Street1;

        /// <summary>
        /// Gets the street2.
        /// </summary>
        /// <value>
        /// The street2.
        /// </value>
        public string Street2 => _location?.Street2;

        /// <summary>
        /// Gets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        public string City => _location?.City;

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public string State => _location?.State;

        /// <summary>
        /// Gets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        public string PostalCode => _location?.PostalCode;

        /// <summary>
        /// Gets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        public string Country => _location?.Country;

        /// <summary>
        /// Gets the county.
        /// </summary>
        /// <value>
        /// The county.
        /// </value>
        public string County => _location?.County;
    }
}
