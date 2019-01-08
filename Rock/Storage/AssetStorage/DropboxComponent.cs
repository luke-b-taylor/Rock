using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using Rock.Attribute;
using Rock.Model;

namespace Rock.Storage.AssetStorage
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Storage.AssetStorage.AssetStorageComponent" />
    [Description( "Dropbox Storage Service" )]
    [Export( typeof( AssetStorageComponent ) )]
    [ExportMetadata( "ComponentName", "Dropbox" )]

    [TextField( name: "Root Folder", description: "Optional root folder. Must be the full path to the root folder starting from the first after the bucket name.", required: false, defaultValue: "", category: "", order: 0, key: "RootFolder" )]
    [TextField( name: "Dropbox Access Token", description: "The access token for the user.", required: true, defaultValue: "rnXqpXPN6xAAAAAAAAAC5Au1X65IetlZjdxbBC9mo2Ue3ELvOmr1PEId7mLKns-P", category: "", order: 1, key: "DropboxAccessToken" )]
    public class DropboxComponent : AssetStorageComponent
    {
        #region Properties

        /// <summary>
        /// Specify the icon for the AssetStorageComponent here. It will display in the folder tree.
        /// Default is server.png.
        /// </summary>
        /// <value>
        /// The component icon path.
        /// </value>
        public override string IconCssClass
        {
            get {
                return "fa fa-dropbox";
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DropboxComponent"/> class.
        /// </summary>
        public DropboxComponent() : base()
        {
        }

        #endregion Constructors

        #region Override Methods

        /// <summary>
        /// Lists the objects from the current root folder.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <returns></returns>
        public override List<Asset> ListObjects( AssetStorageProvider assetStorageProvider )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, "RootFolder" ) );
            return ListObjects( assetStorageProvider, new Asset { Type = AssetType.Folder, Uri = rootFolder } );
        }

        /// <summary>
        /// Lists the objects. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If key and name are not provided then list all objects from the current RootFolder.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public override List<Asset> ListObjects( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            try
            {
                List<Asset> assets = GetList( assetStorageProvider, asset, true, true );
                return assets;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Creates the download link.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override string CreateDownloadLink( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            try
            {
                DropboxClient client = GetDropboxClient( assetStorageProvider );
                var repsonse = client.Files.GetTemporaryLinkAsync( asset.Key ).Result;

                if ( repsonse != null )
                {
                    return repsonse.Link;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
            return string.Empty;
        }

        /// <summary>
        /// Creates a folder. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key is provided it MUST use the full path, RootFolder is not used.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public override bool CreateFolder( AssetStorageProvider assetStorageProvider, Asset asset )
        {

            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, "RootFolder" ) );
            try
            {
                DropboxClient client = GetDropboxClient( assetStorageProvider );
                var response = client.Files.CreateFolderV2Async( asset.Key ).Result;
                if ( response != null )
                {
                    return true;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }

            return false;
        }

        /// <summary>
        /// Deletes the asset. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key is provided then it MUST use the full path, RootFolder is not used.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override bool DeleteAsset( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            DropboxClient client = GetDropboxClient( assetStorageProvider );

            try
            {
                if ( asset.Key.EndsWith( "/" ) )
                {
                    asset.Key = asset.Key.TrimEnd( '/' );
                }

                var response = client.Files.DeleteV2Async( asset.Key ).Result;
                if ( asset.Type == AssetType.File )
                {
                    DeleteImageThumbnail( assetStorageProvider, asset );
                }
                return true;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Returns an asset with the stream of the specified file and creates a thumbnail.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override Asset GetObject( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            return GetObject( assetStorageProvider, asset, true );
        }

        /// <summary>
        /// Returns an asset with the stream of the specified file with the option to create a thumbnail.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <param name="createThumbnail">if set to <c>true</c> [create thumbnail].</param>
        /// <returns></returns>
        public override Asset GetObject( AssetStorageProvider assetStorageProvider, Asset asset, bool createThumbnail )
        {

            try
            {
                DropboxClient client = GetDropboxClient( assetStorageProvider );
                var response = client.Files.DownloadAsync( asset.Key ).Result;
                return CreateAssetFromMetaData( assetStorageProvider, response.Response, response.GetContentAsStreamAsync().Result, createThumbnail );

            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        public override string GetThumbnail( AssetStorageProvider assetStorageProvider, string keyUrl, DateTime? lastModifiedDateTime )
        {
            string name = GetNameFromKey( keyUrl );
            string path = GetPathFromKey( keyUrl );

            string mimeType = System.Web.MimeMapping.GetMimeMapping( name );
            if ( !mimeType.StartsWith( "image/" ) )
            {
                return GetFileTypeIcon( path );
            }

            // check if thumbnail exists
            string thumbDir = $"{ThumbnailRootPath}/{assetStorageProvider.Id}/{path}";
            Directory.CreateDirectory( FileSystemCompontHttpContext.Server.MapPath( thumbDir ) );

            string virtualThumbPath = Path.Combine( thumbDir, name );
            string physicalThumbPath = FileSystemCompontHttpContext.Server.MapPath( virtualThumbPath );

            if ( File.Exists( physicalThumbPath ) )
            {
                var thumbLastModDate = File.GetLastWriteTimeUtc( physicalThumbPath );
                if ( lastModifiedDateTime <= thumbLastModDate )
                {
                    // thumbnail is still good so just return the virtual file path.
                    return virtualThumbPath;
                }
            }

            CreateImageThumbnail( assetStorageProvider, new Asset { Name = name, Key = keyUrl, Type = AssetType.File }, physicalThumbPath, false );

            return virtualThumbPath;
        }

        public override List<Asset> ListFilesInFolder( AssetStorageProvider assetStorageProvider )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, "RootFolder" ) );
            return ListFilesInFolder( assetStorageProvider, new Asset { Type = AssetType.Folder, Key = rootFolder } );
        }

        public override List<Asset> ListFilesInFolder( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            try
            {
                List<Asset> assets = GetList( assetStorageProvider, asset, true, false );
                return assets;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        public override List<Asset> ListFoldersInFolder( AssetStorageProvider assetStorageProvider )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, "RootFolder" ) );
            return ListFoldersInFolder( assetStorageProvider, new Asset { Type = AssetType.Folder, Key = rootFolder } );
        }

        public override List<Asset> ListFoldersInFolder( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            try
            {
                List<Asset> assets = GetList( assetStorageProvider, asset, false, true );
                return assets;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Gets the objects in folder without recursion. i.e. will get the list of files
        /// and folders in the folder but not the contents of the subfolders.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage Provider.</param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override List<Asset> ListObjectsInFolder( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            try
            {
                List<Asset> assets = GetList( assetStorageProvider, asset, true, true );
                return assets;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Renames the asset.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset">The asset.</param>
        /// <param name="newName">The new name.</param>
        /// <returns></returns>
        public override bool RenameAsset( AssetStorageProvider assetStorageProvider, Asset asset, string newName )
        {
            try
            {
                DropboxClient client = GetDropboxClient( assetStorageProvider );

                var sourcePath = asset.Key;
                var destinationPath = GetPathFromKey( asset.Key ) + newName;
                var response = client.Files.MoveV2Async( sourcePath, destinationPath ).Result;
                if ( response.Metadata != null )
                {
                    return true;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }

            return false;
        }

        /// <summary>
        /// Uploads a file. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If a key is provided it MUST use the full path, RootFolder is not used.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override bool UploadObject( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            try
            {
                DropboxClient client = GetDropboxClient( assetStorageProvider );
                var response = client.Files.UploadAsync( asset.Key,
                    WriteMode.Overwrite.Instance,
                    body: asset.AssetStream ).Result;

                if ( response != null )
                {
                    return true;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }

            return false;
        }

        /// <summary>
        /// Deletes the image thumbnail for the provided Asset. If the asset is a file then the singel thumbnail
        /// is deleted. If the asset is a directory then a recurrsive delete is done.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        protected override void DeleteImageThumbnail( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            base.DeleteImageThumbnail( assetStorageProvider, asset );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns an Dropbox Client obj using the settings in the provided AssetStorageProvider obj.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <returns></returns>
        private DropboxClient GetDropboxClient( AssetStorageProvider assetStorageProvider )
        {

            string dropboxAccessKey = GetAttributeValue( assetStorageProvider, "DropboxAccessToken" );

            return new DropboxClient( "rnXqpXPN6xAAAAAAAAAC5Au1X65IetlZjdxbBC9mo2Ue3ELvOmr1PEId7mLKns-P" );
        }


        /// <summary>
        /// Creates the asset from the AWS S3Object.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="item">The s3 object.</param>
        /// <param name="regionEndpoint">The region endpoint.</param>
        /// <returns></returns>
        private Asset CreateAssetFromDropboxObject( AssetStorageProvider assetStorageProvider, Metadata item )
        {
            var assetType = item.IsFolder ? AssetType.Folder : AssetType.File;
            var asset = new Asset
            {
                Name = item.Name,
                Type = assetType,
                Uri = item.PathLower,
            };
            if ( assetType == AssetType.File )
            {
                var file = item.AsFile;
                asset.FileSize = ( long ) file.Size;
                asset.LastModifiedDateTime = file.ServerModified;
                asset.Key = item.PathDisplay;
                asset.IconPath = GetThumbnail( assetStorageProvider, item.PathDisplay, file.ServerModified );
            }
            else
            {
                asset.Key = item.PathDisplay + "/";
            }
            return asset;
        }

        /// <summary>
        /// Creates the asset from AWS S3 Client GetObjectResponse.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="response">The response.</param>
        /// <param name="regionEndpoint">The region endpoint.</param>
        /// <param name="createThumbnail">if set to <c>true</c> [create thumbnail].</param>
        /// <returns></returns>
        private Asset CreateAssetFromMetaData( AssetStorageProvider assetStorageProvider, FileMetadata response, Stream stream, bool createThumbnail )
        {
            return new Asset
            {
                Name = response.Name,
                Key = response.PathDisplay,
                Uri = response.PathDisplay,
                Type = AssetType.File,
                IconPath = createThumbnail == true ? GetThumbnail( assetStorageProvider, response.PathDisplay, response.ServerModified ) : GetFileTypeIcon( response.PathDisplay ),
                FileSize = ( long ) response.Size,
                LastModifiedDateTime = response.ServerModified,
                AssetStream = stream
            };
        }

        /// <summary>
        /// Gets the icon for the file type based on the extension of the provided file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        protected virtual string GetFileTypeIcon( string fileName )
        {
            string fileExtension = Path.GetExtension( fileName ).TrimStart( '.' );
            string virtualThumbnailFilePath = string.Format( "/Assets/Icons/FileTypes/{0}.png", fileExtension );
            string thumbnailFilePath = FileSystemCompontHttpContext.Request.MapPath( virtualThumbnailFilePath );

            if ( !File.Exists( thumbnailFilePath ) )
            {
                virtualThumbnailFilePath = "/Assets/Icons/FileTypes/other.png";
                thumbnailFilePath = FileSystemCompontHttpContext.Request.MapPath( virtualThumbnailFilePath );
            }

            return virtualThumbnailFilePath;
        }

        private List<Asset> GetList( AssetStorageProvider assetStorageProvider, Asset asset, bool isFile, bool isFolder )
        {
            DropboxClient client = GetDropboxClient( assetStorageProvider );

            var assets = new List<Asset>();

            ListFolderResult response = null;

            do
            {
                if ( response != null && response.HasMore )
                {
                    response = client.Files.ListFolderContinueAsync( response.Cursor ).Result;
                }
                else
                {
                    response = client.Files.ListFolderAsync( asset.Key ?? string.Empty ).Result;
                }

                // show folders then files
                foreach ( var item in response.Entries.Where( a => ( ( a.IsFile && isFile ) || ( a.IsFolder && isFolder ) ) && !a.IsDeleted ) )
                {
                    if ( item.Name == null )
                    {
                        continue;
                    }

                    var responseAsset = CreateAssetFromDropboxObject( assetStorageProvider, item );
                    assets.Add( responseAsset );
                }
            } while ( response.HasMore );

            return assets;
        }

        /// <summary>
        /// Gets the name from key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string GetNameFromKey( string key )
        {
            if ( key.LastIndexOf( '/' ) < 1 )
            {
                return key;
            }

            string[] pathSegments = key.Split( '/' );

            if ( key.EndsWith( "/" ) )
            {
                return pathSegments[pathSegments.Length - 2];
            }

            return pathSegments[pathSegments.Length - 1];
        }

        /// <summary>
        /// Gets the path from key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The file prefix used by AWS to mimic a folder structure.</returns>
        private string GetPathFromKey( string key )
        {
            int i = key.LastIndexOf( '/' );
            if ( i < 1 )
            {
                return string.Empty;
            }

            return key.Substring( 0, i + 1 );
        }


        #endregion
    }
}
