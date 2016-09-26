using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Wutnu.Search
{
    public enum SearchEngineType
    {
        solr,
        AzureSearch
    }

    public class SearchEngine
    {
        SearchEngineType _type;
        _type = SearchEngineType.AzureSearch;


    }
    public class MySearch : ISearchServiceClient
    {
        public static SearchEngine SearchEngine { get; set; }

        public MySearch()
        {

        }

        string ISearchServiceClient.AcceptLanguage
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        string ISearchServiceClient.ApiVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        Uri ISearchServiceClient.BaseUri
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        ServiceClientCredentials ISearchServiceClient.Credentials
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        IDataSourcesOperations ISearchServiceClient.DataSources
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        JsonSerializerSettings ISearchServiceClient.DeserializationSettings
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        IIndexersOperations ISearchServiceClient.Indexers
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        IIndexesOperations ISearchServiceClient.Indexes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        int? ISearchServiceClient.LongRunningOperationRetryTimeout
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        SearchCredentials ISearchServiceClient.SearchCredentials
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        JsonSerializerSettings ISearchServiceClient.SerializationSettings
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
