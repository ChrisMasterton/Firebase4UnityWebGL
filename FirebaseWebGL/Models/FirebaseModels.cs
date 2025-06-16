using System;
using System.Collections.Generic;
using UnityEngine;

namespace Firebase.Models
{
    [Serializable]
    public class DatabaseResponse<T>
    {
        public T data;
        public string path;
        public long timestamp;
    }
    
    [Serializable]
    public class FirestoreDocument
    {
        public string name;
        public Dictionary<string, FirestoreValue> fields;
        public string createTime;
        public string updateTime;
        
        public T GetField<T>(string fieldName)
        {
            if (fields == null || !fields.ContainsKey(fieldName))
                return default(T);
                
            return fields[fieldName].GetValue<T>();
        }
        
        public void SetField<T>(string fieldName, T value)
        {
            if (fields == null)
                fields = new Dictionary<string, FirestoreValue>();
                
            fields[fieldName] = FirestoreValue.Create(value);
        }
    }
    
    [Serializable]
    public class FirestoreValue
    {
        public string nullValue;
        public bool? booleanValue;
        public string integerValue;
        public string doubleValue;
        public string timestampValue;
        public string stringValue;
        public string bytesValue;
        public string referenceValue;
        public FirestoreGeoPoint geoPointValue;
        public FirestoreArrayValue arrayValue;
        public FirestoreMapValue mapValue;
        
        public T GetValue<T>()
        {
            var type = typeof(T);
            
            if (type == typeof(string))
                return (T)(object)stringValue;
            else if (type == typeof(int) || type == typeof(int?))
                return int.TryParse(integerValue, out var intVal) ? (T)(object)intVal : default(T);
            else if (type == typeof(long) || type == typeof(long?))
                return long.TryParse(integerValue, out var longVal) ? (T)(object)longVal : default(T);
            else if (type == typeof(float) || type == typeof(float?))
                return float.TryParse(doubleValue, out var floatVal) ? (T)(object)floatVal : default(T);
            else if (type == typeof(double) || type == typeof(double?))
                return double.TryParse(doubleValue, out var doubleVal) ? (T)(object)doubleVal : default(T);
            else if (type == typeof(bool) || type == typeof(bool?))
                return booleanValue.HasValue ? (T)(object)booleanValue.Value : default(T);
            else if (type == typeof(DateTime) || type == typeof(DateTime?))
                return DateTime.TryParse(timestampValue, out var dateVal) ? (T)(object)dateVal : default(T);
                
            return default(T);
        }
        
        public static FirestoreValue Create<T>(T value)
        {
            var firestoreValue = new FirestoreValue();
            var type = typeof(T);
            
            if (value == null)
            {
                firestoreValue.nullValue = null;
            }
            else if (type == typeof(string))
            {
                firestoreValue.stringValue = value.ToString();
            }
            else if (type == typeof(int) || type == typeof(long))
            {
                firestoreValue.integerValue = value.ToString();
            }
            else if (type == typeof(float) || type == typeof(double))
            {
                firestoreValue.doubleValue = value.ToString();
            }
            else if (type == typeof(bool))
            {
                firestoreValue.booleanValue = (bool)(object)value;
            }
            else if (type == typeof(DateTime))
            {
                firestoreValue.timestampValue = ((DateTime)(object)value).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            }
            else
            {
                firestoreValue.stringValue = JsonUtility.ToJson(value);
            }
            
            return firestoreValue;
        }
    }
    
    [Serializable]
    public class FirestoreGeoPoint
    {
        public double latitude;
        public double longitude;
    }
    
    [Serializable]
    public class FirestoreArrayValue
    {
        public FirestoreValue[] values;
    }
    
    [Serializable]
    public class FirestoreMapValue
    {
        public Dictionary<string, FirestoreValue> fields;
    }
    
    [Serializable]
    public class FirestoreListDocumentsResponse
    {
        public FirestoreDocument[] documents;
        public string nextPageToken;
    }
    
    [Serializable]
    public class FirestoreQuery
    {
        public FirestoreStructuredQuery structuredQuery;
    }
    
    [Serializable]
    public class FirestoreStructuredQuery
    {
        public FirestoreCollectionSelector[] from;
        public FirestoreFilter where;
        public FirestoreOrder[] orderBy;
        public int? limit;
        public int? offset;
        public string startAt;
        public string endAt;
    }
    
    [Serializable]
    public class FirestoreCollectionSelector
    {
        public string collectionId;
        public bool? allDescendants;
    }
    
    [Serializable]
    public class FirestoreFilter
    {
        public FirestoreCompositeFilter compositeFilter;
        public FirestoreFieldFilter fieldFilter;
        public FirestoreUnaryFilter unaryFilter;
    }
    
    [Serializable]
    public class FirestoreCompositeFilter
    {
        public string op; // "AND" or "OR"
        public FirestoreFilter[] filters;
    }
    
    [Serializable]
    public class FirestoreFieldFilter
    {
        public FirestoreFieldReference field;
        public string op; // "EQUAL", "NOT_EQUAL", "LESS_THAN", etc.
        public FirestoreValue value;
    }
    
    [Serializable]
    public class FirestoreUnaryFilter
    {
        public string op; // "IS_NULL", "IS_NOT_NULL"
        public FirestoreFieldReference field;
    }
    
    [Serializable]
    public class FirestoreFieldReference
    {
        public string fieldPath;
    }
    
    [Serializable]
    public class FirestoreOrder
    {
        public FirestoreFieldReference field;
        public string direction; // "ASCENDING" or "DESCENDING"
    }
    
    [Serializable]
    public class StorageObject
    {
        public string name;
        public string bucket;
        public string generation;
        public string metageneration;
        public string contentType;
        public string timeCreated;
        public string updated;
        public string storageClass;
        public string size;
        public string md5Hash;
        public string mediaLink;
        public Dictionary<string, string> metadata;
        public string downloadTokens;
    }
    
    [Serializable]
    public class StorageUploadResponse
    {
        public string name;
        public string bucket;
        public string downloadTokens;
    }
    
    [Serializable]
    public class FcmMessage
    {
        public string to;
        public string[] registration_ids;
        public FcmNotification notification;
        public Dictionary<string, string> data;
        public string priority;
        public bool content_available;
        public string collapse_key;
        public int time_to_live;
    }
    
    [Serializable]
    public class FcmNotification
    {
        public string title;
        public string body;
        public string icon;
        public string sound;
        public string tag;
        public string color;
        public string click_action;
        public string body_loc_key;
        public string[] body_loc_args;
        public string title_loc_key;
        public string[] title_loc_args;
    }
    
    [Serializable]
    public class FcmResponse
    {
        public long multicast_id;
        public int success;
        public int failure;
        public int canonical_ids;
        public FcmResult[] results;
    }
    
    [Serializable]
    public class FcmResult
    {
        public string message_id;
        public string registration_id;
        public string error;
    }
}