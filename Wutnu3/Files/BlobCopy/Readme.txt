BlobCopy Utility

This utility fetches a SAS token with authorization to upload, list, and download files from
an assigned container in Azure Blob Storage. 

It is designed to work exclusively with its associated web application - it is not a 
stand-alone blob copy tool.

SETTINGS (in api.config)
   ApiUrl: the server URL used by the utility to retrieve authorization tokens.
   ApiKey: the key used by the utility to authenticate calls to retrieve auth tokens.

   NOTE: this ApiKey is also specified in the container owner's profile page. If the token 
   is regenerated, the new value will have to be retrieved and updated in api.config.

USAGE
   blobcopy put <path_to_file>

   blobcopy list
       NOTE: files are case-sensitive.

   blobcopy get <filename in blob>

   blobcopy delete <filename in blob>

INSTALLATION
	The contents of the zip file should be extracted TOGETHER to a folder.
