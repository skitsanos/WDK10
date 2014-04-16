CouchdB Client for .NET and Mono
---
>A CouchDB server hosts named databases, which store documents. Each document is uniquely named in the database, and CouchDB provides a RESTful HTTP API for reading and updating (add, edit, delete) database documents.

>Documents are the primary unit of data in CouchDB and consist of any number of fields and attachments. Documents also include metadata that’s maintained by the database system. Document fields are uniquely named and contain values of varying types (text, number, boolean, lists, etc), and there is no set limit to text size or element count.

>The CouchDB document update model is lockless and optimistic. Document edits are made by client applications loading documents, applying changes, and saving them back to the database. If another client editing the same document saves their changes first, the client gets an edit conflict error on save. To resolve the update conflict, the latest document version can be opened, the edits reapplied and the update tried again.

>Document updates (add, edit, delete) are all or nothing, either succeeding entirely or failing completely. The database never contains partially saved or edited documents.

>For downloads and more information about CouchDB, please visit [http://couchdb.apache.org](http://couchdb.apache.org)

On the summer of 2011 we brought WDK.API.CouchDB to help .NET and Mono developers to query and manage Apache CouchDB databases, views, data documents and etc functionality available today within CouchDB. Today this library powers tons of small and enterprise size web sites and web applications and it is a heart for the only CouchDB IDE available today -- Kanapes IDE [http://kanapeside.com](http://kanapeside.com) Currently Kanapes IDE exists for Windows only and you can get it for FREE.

---


###Using the library

To start, make sure you have included wdk.api.couchdb.dll into your Project References. 

```csharp
using WDK.API.CouchDB;
```

Now you can create your CouchDB client instance:

```csharp
var couchdb = new Database();
```


####Database Management

######Get Databases

```csharp
var dbs = couchdb.getDatabases();
```

######Check if Database exists

```csharp
bool isExists = couchdb.databaseExists(DB_NAME);
```

Where _DB_NAME_ is the name of the database you want to check on existence.

######Create Database

```csharp
bool created = couchdb.createDatabase(DB_NAME);
```

Where _DB_NAME_ is the name of the database you want to create. Returns _true_ if database was created successfully, otherwise _false_.

######Delete Database

```csharp
bool deleted = couchdb.deleteDatabase(DB_NAME);
```

Where _DB_NAME_ is the name of the database you want to delete. Returns _true_ if database was deleted successfully, otherwise it will throw _InvalidServerResponseException_.

####Data documents

######Get All Documents
Get information on all the documents in the given database.

```csharp
List<DocumentInfo> docs = couchdb.getAllDocuments(DB_NAME);
```
