using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Firebase.Core;
using Firebase.Models;

namespace Firebase.Firestore
{
    public class FirebaseFirestore
    {
        private readonly FirebaseConfig config;
        private readonly FirebaseAuth auth;
        
        public FirebaseFirestore(FirebaseConfig config, FirebaseAuth auth = null)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.auth = auth;
        }
        
        public CollectionReference Collection(string collectionPath)
        {
            return new CollectionReference(this, collectionPath);
        }
        
        public DocumentReference Document(string documentPath)
        {
            return new DocumentReference(this, documentPath);
        }
        
        public async Task<FirestoreDocument> GetDocumentAsync(string documentPath)
        {
            var url = $"{config.FirestoreBaseUrl}/{documentPath.TrimStart('/')}";
            
            using var request = FirebaseUtils.CreateRequest(url, "GET");
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
                
            return FirebaseUtils.FromJson<FirestoreDocument>(request.downloadHandler.text);
        }
        
        public async Task<FirestoreDocument[]> GetCollectionAsync(string collectionPath, int? pageSize = null, string pageToken = null)
        {
            var url = $"{config.FirestoreBaseUrl}/{collectionPath.TrimStart('/')}";
            
            var parameters = new Dictionary<string, string>();
            if (pageSize.HasValue)
                parameters["pageSize"] = pageSize.Value.ToString();
            if (!string.IsNullOrEmpty(pageToken))
                parameters["pageToken"] = pageToken;
                
            url += FirebaseUtils.BuildQueryParams(parameters);
            
            using var request = FirebaseUtils.CreateRequest(url, "GET");
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
                
            var response = FirebaseUtils.FromJson<FirestoreListDocumentsResponse>(request.downloadHandler.text);
            return response.documents ?? new FirestoreDocument[0];
        }
        
        public async Task<FirestoreDocument> CreateDocumentAsync(string collectionPath, FirestoreDocument document, string documentId = null)
        {
            string url;
            if (!string.IsNullOrEmpty(documentId))
            {
                url = $"{config.FirestoreBaseUrl}/{collectionPath.TrimStart('/')}/{documentId}";
            }
            else
            {
                url = $"{config.FirestoreBaseUrl}/{collectionPath.TrimStart('/')}";
            }
            
            var json = FirebaseUtils.ToJson(document);
            using var request = FirebaseUtils.CreateRequest(url, "POST", json);
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
                
            return FirebaseUtils.FromJson<FirestoreDocument>(request.downloadHandler.text);
        }
        
        public async Task<FirestoreDocument> SetDocumentAsync(string documentPath, FirestoreDocument document, bool merge = false)
        {
            var url = $"{config.FirestoreBaseUrl}/{documentPath.TrimStart('/')}";
            
            if (merge)
            {
                var parameters = new Dictionary<string, string> { { "updateMask.fieldPaths", "*" } };
                url += FirebaseUtils.BuildQueryParams(parameters);
            }
            
            var json = FirebaseUtils.ToJson(document);
            using var request = FirebaseUtils.CreateRequest(url, merge ? "PATCH" : "PUT", json);
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
                
            return FirebaseUtils.FromJson<FirestoreDocument>(request.downloadHandler.text);
        }
        
        public async Task<FirestoreDocument> UpdateDocumentAsync(string documentPath, Dictionary<string, FirestoreValue> fields, string[] updateMask = null)
        {
            var url = $"{config.FirestoreBaseUrl}/{documentPath.TrimStart('/')}";
            
            var parameters = new Dictionary<string, string>();
            if (updateMask != null && updateMask.Length > 0)
            {
                for (int i = 0; i < updateMask.Length; i++)
                {
                    parameters[$"updateMask.fieldPaths"] = updateMask[i];
                }
            }
            
            url += FirebaseUtils.BuildQueryParams(parameters);
            
            var document = new FirestoreDocument { fields = fields };
            var json = FirebaseUtils.ToJson(document);
            
            using var request = FirebaseUtils.CreateRequest(url, "PATCH", json);
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
                
            return FirebaseUtils.FromJson<FirestoreDocument>(request.downloadHandler.text);
        }
        
        public async Task DeleteDocumentAsync(string documentPath)
        {
            var url = $"{config.FirestoreBaseUrl}/{documentPath.TrimStart('/')}";
            
            using var request = FirebaseUtils.CreateRequest(url, "DELETE");
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
        }
        
        public async Task<FirestoreDocument[]> QueryAsync(FirestoreQuery query)
        {
            var url = $"{config.FirestoreBaseUrl}:runQuery";
            var json = FirebaseUtils.ToJson(query);
            
            using var request = FirebaseUtils.CreateRequest(url, "POST", json);
            await AddAuthIfNeeded(request);
            await SendRequestAsync(request);
            
            var exception = FirebaseUtils.HandleError(request);
            if (exception != null)
                throw exception;
                
            var results = FirebaseUtils.FromJson<QueryResult[]>(request.downloadHandler.text);
            return results?.Where(r => r.document != null).Select(r => r.document).ToArray() ?? new FirestoreDocument[0];
        }
        
        private async Task AddAuthIfNeeded(UnityWebRequest request)
        {
            if (auth != null && auth.IsSignedIn)
            {
                try
                {
                    var token = await auth.GetIdTokenAsync();
                    FirebaseUtils.SetAuthHeader(request, token);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to add auth token: {ex.Message}");
                }
            }
        }
        
        private async Task SendRequestAsync(UnityWebRequest request)
        {
            var operation = request.SendWebRequest();
            
            while (!operation.isDone)
            {
                await Task.Yield();
            }
        }
    }
    
    public class CollectionReference
    {
        private readonly FirebaseFirestore firestore;
        private readonly string path;
        
        public string Id => path.Split('/')[^1];
        public string Path => path;
        
        internal CollectionReference(FirebaseFirestore firestore, string path)
        {
            this.firestore = firestore;
            this.path = path.Trim('/');
        }
        
        public DocumentReference Document(string documentPath = null)
        {
            var docPath = string.IsNullOrEmpty(documentPath) 
                ? $"{path}/{FirebaseUtils.GenerateRandomString()}"
                : $"{path}/{documentPath}";
            return new DocumentReference(firestore, docPath);
        }
        
        public async Task<FirestoreDocument[]> GetAsync()
        {
            return await firestore.GetCollectionAsync(path);
        }
        
        public async Task<DocumentReference> AddAsync(Dictionary<string, object> data)
        {
            var document = new FirestoreDocument
            {
                fields = data.ToDictionary(kvp => kvp.Key, kvp => FirestoreValue.Create(kvp.Value))
            };
            
            var result = await firestore.CreateDocumentAsync(path, document);
            var docId = result.name.Split('/')[^1];
            return Document(docId);
        }
        
        public Query Where(string field, string op, object value)
        {
            return new Query(firestore, path).Where(field, op, value);
        }
        
        public Query OrderBy(string field, bool descending = false)
        {
            return new Query(firestore, path).OrderBy(field, descending);
        }
        
        public Query Limit(int limit)
        {
            return new Query(firestore, path).Limit(limit);
        }
    }
    
    public class DocumentReference
    {
        private readonly FirebaseFirestore firestore;
        private readonly string path;
        
        public string Id => path.Split('/')[^1];
        public string Path => path;
        public CollectionReference Parent => new CollectionReference(firestore, GetParentPath(path));
        
        internal DocumentReference(FirebaseFirestore firestore, string path)
        {
            this.firestore = firestore;
            this.path = path.Trim('/');
        }
        
        public CollectionReference Collection(string collectionPath)
        {
            return new CollectionReference(firestore, $"{path}/{collectionPath}");
        }
        
        public async Task<FirestoreDocument> GetAsync()
        {
            return await firestore.GetDocumentAsync(path);
        }
        
        public async Task<T> GetAsync<T>() where T : new()
        {
            var document = await GetAsync();
            if (document?.fields == null)
                return default(T);
                
            var obj = new T();
            var type = typeof(T);
            var properties = type.GetProperties();
            
            foreach (var prop in properties)
            {
                if (document.fields.ContainsKey(prop.Name))
                {
                    var value = document.fields[prop.Name].GetValue<object>();
                    if (value != null && prop.CanWrite)
                    {
                        prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
                    }
                }
            }
            
            return obj;
        }
        
        public async Task SetAsync(Dictionary<string, object> data, bool merge = false)
        {
            var document = new FirestoreDocument
            {
                fields = data.ToDictionary(kvp => kvp.Key, kvp => FirestoreValue.Create(kvp.Value))
            };
            
            await firestore.SetDocumentAsync(path, document, merge);
        }
        
        public async Task SetAsync<T>(T data, bool merge = false)
        {
            var dict = ConvertObjectToDictionary(data);
            await SetAsync(dict, merge);
        }
        
        public async Task UpdateAsync(Dictionary<string, object> data)
        {
            var fields = data.ToDictionary(kvp => kvp.Key, kvp => FirestoreValue.Create(kvp.Value));
            var updateMask = data.Keys.ToArray();
            
            await firestore.UpdateDocumentAsync(path, fields, updateMask);
        }
        
        public async Task DeleteAsync()
        {
            await firestore.DeleteDocumentAsync(path);
        }
        
        private Dictionary<string, object> ConvertObjectToDictionary<T>(T obj)
        {
            var dict = new Dictionary<string, object>();
            var type = typeof(T);
            var properties = type.GetProperties();
            
            foreach (var prop in properties)
            {
                if (prop.CanRead)
                {
                    var value = prop.GetValue(obj);
                    if (value != null)
                    {
                        dict[prop.Name] = value;
                    }
                }
            }
            
            return dict;
        }
        
        private string GetParentPath(string childPath)
        {
            var lastSlash = childPath.LastIndexOf('/');
            return lastSlash > 0 ? childPath.Substring(0, lastSlash) : "";
        }
    }
    
    public class Query
    {
        private readonly FirebaseFirestore firestore;
        private readonly string collectionPath;
        private readonly List<FirestoreFilter> filters = new List<FirestoreFilter>();
        private readonly List<FirestoreOrder> orders = new List<FirestoreOrder>();
        private int? limitValue;
        
        internal Query(FirebaseFirestore firestore, string collectionPath)
        {
            this.firestore = firestore;
            this.collectionPath = collectionPath;
        }
        
        public Query Where(string field, string op, object value)
        {
            var filter = new FirestoreFilter
            {
                fieldFilter = new FirestoreFieldFilter
                {
                    field = new FirestoreFieldReference { fieldPath = field },
                    op = op.ToUpper(),
                    value = FirestoreValue.Create(value)
                }
            };
            
            filters.Add(filter);
            return this;
        }
        
        public Query OrderBy(string field, bool descending = false)
        {
            var order = new FirestoreOrder
            {
                field = new FirestoreFieldReference { fieldPath = field },
                direction = descending ? "DESCENDING" : "ASCENDING"
            };
            
            orders.Add(order);
            return this;
        }
        
        public Query Limit(int limit)
        {
            limitValue = limit;
            return this;
        }
        
        public async Task<FirestoreDocument[]> GetAsync()
        {
            var query = new FirestoreQuery
            {
                structuredQuery = new FirestoreStructuredQuery
                {
                    from = new[]
                    {
                        new FirestoreCollectionSelector { collectionId = collectionPath.Split('/')[^1] }
                    },
                    where = filters.Count > 1 
                        ? new FirestoreFilter { compositeFilter = new FirestoreCompositeFilter { op = "AND", filters = filters.ToArray() } }
                        : filters.FirstOrDefault(),
                    orderBy = orders.ToArray(),
                    limit = limitValue
                }
            };
            
            return await firestore.QueryAsync(query);
        }
    }
    
    [Serializable]
    internal class QueryResult
    {
        public FirestoreDocument document;
        public string readTime;
        public int skippedResults;
    }
}