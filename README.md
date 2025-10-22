# 🧩 StringAnalyzer API

A **C#/.NET 8 RESTful Web API** that analyzes strings and stores their computed properties.  
The service allows users to create, retrieve, filter, and delete analyzed strings — supporting both parameterized and **natural language** filtering.

---

## 🚀 Features

For each analyzed string, the API computes and stores the following properties:

| Property | Description |
|-----------|--------------|
| **length** | Number of characters in the string |
| **is_palindrome** | Boolean indicating if the string reads the same forwards and backwards (case-insensitive) |
| **unique_characters** | Count of distinct characters |
| **word_count** | Number of words separated by whitespace |
| **sha256_hash** | SHA-256 hash of the string for unique identification |
| **character_frequency_map** | Dictionary mapping each character to its occurrence count |

---

## 🛠️ Tech Stack

- **Framework:** .NET 8 Web API  
- **Language:** C#  
- **Database:** In-memory Database  
- **ORM:** Entity Framework Core  
- **Hashing:** `System.Security.Cryptography` (SHA256)  
- **JSON Serialization:** System.Text.Json    

---

## 📚 API Endpoints

## 1️⃣ Create / Analyze String

Analyzes a new string, computes its properties, and stores the resulting analysis.  
If the string already exists, it returns a **409 Conflict** error.

---

### **Method**
`POST`

### **Path**
`/strings`

### **Description**
Submits a string for analysis and persistence.

---

### **Request Body** (`application/json`)
```json
{
  "value": "string to analyze"
}
```

---

#### Success Response (201 Created):
```
{
  "id": "sha256_hash_value",
  "value": "string to analyze",
  "properties": {
    "length": 17,
    "is_palindrome": false,
    "unique_characters": 12,
    "word_count": 3,
    "sha256_hash": "abc123...",
    "character_frequency_map": {
      "s": 2,
      "t": 3,
      "r": 2
    }
  },
  "created_at": "2025-08-27T10:00:00Z"
}
```

---

#### ❌ Error Responses

| **Status** | **Error Description** |
|-------------|------------------------|
| **409 Conflict** | String already exists in the system (based on its hash). |
| **400 Bad Request** | Invalid request body or missing `"value"` field. |
| **422 Unprocessable Entity** | Invalid data type for `"value"` (must be a string). |


----

## 2️⃣ Get Specific String

Retrieves the full analysis record for a specific string using the string's raw value as the identifier in the URL path.

---
### **Endpoint Information**

| **Method** | **Path** | **Description** |
|-----------------|--------------|---------------------|
|`GET` | `/strings/{string_value}` | Retrieves the analysis by string content. |

---

### **Success Response** (`200 OK`)
Returns the full analysis object (same structure as the `POST /strings` **201 Created** response).

---

### **Error Response**

| **Status** | **Error Description** |
|-------------|------------------------|
| **404 Not Found** | String analysis does not exist in the system. |

---

## 3️⃣ Get All Strings with Filtering

Retrieves a list of analyzed strings, optionally applying filters based on the computed properties.

---

### **Endpoint Information**

| **Method** | **Path** | **Description** |
|-------------|-----------|------------------|
| **GET** | `/strings?query_params` | Filters the stored strings by property values. |


---

### **Query Parameters (Optional)**

| **Parameter** | **Type** | **Example** | **Description** |
|----------------|-----------|--------------|------------------|
| `is_palindrome` | Boolean | `true` | Filters for palindromes (or non-palindromes). |
| `min_length` | Integer | `5` | Minimum length (inclusive). |
| `max_length` | Integer | `20` | Maximum length (inclusive). |
| `word_count` | Integer | `2` | Exact word count. |
| `contains_character` | String | `a` | Filters for strings containing this character (case-insensitive). |

---

### **Success Response** (`200 OK`)
```json
{
  "data": [
    {
      "id": "hash1",
      "value": "string1",
      "properties": { /* ... */ },
      "created_at": "2025-08-27T10:00:00Z"
    }
  ],
  "count": 1,
  "filters_applied": {
    "is_palindrome": true,
    "min_length": 5
  }
}
```

---

## 4️⃣ Natural Language Filtering

Allows users to query the string database using **human-readable language**.  
The service interprets the query and converts it into structured filters.

---

### **Endpoint Information**

| **Method** | **Path** | **Description** |
|-------------|-----------|------------------|
| **GET** | `/strings/filter-by-natural-language?query={natural_language_query}` | Interprets a natural language query into structured filters. |

---

### **Example Request**
```
GET
/strings/filter-by-natural-language?query=all%20single%20word%20palindromic%20strings
```

### **Success Response** (`200 OK`)
```json
{
  "data": [ /* array of matching strings */ ],
  "count": 3,
  "interpreted_query": {
    "original": "all single word palindromic strings",
    "parsed_filters": {
      "word_count": 1,
      "is_palindrome": true
    }
  }
}
```

### **Supported Query Interpretation Examples**

| **Natural Language Query** | **Interpreted Filters** |
|-----------------------------|--------------------------|
| `"all single word palindromic strings"` | `word_count=1`, `is_palindrome=true` |
| `"strings longer than 10 characters"` | `min_length=11` |
| `"palindromic strings that contain the first vowel"` | `is_palindrome=true`, `contains_character=a` |
| `"strings containing the letter z"` | `contains_character=z` |

---

### **Error Responses**

| **Status** | **Error Description** |
|-------------|------------------------|
| **400 Bad Request** | Unable to parse the natural language query into any recognizable filter. |
| **422 Unprocessable Entity** | Query parsed but resulted in conflicting or illogical filters (e.g., `min_length=5` and `max_length=2`). |

---

## **5. Delete String**

Removes an analyzed string record from the system using the string's raw value as the identifier.

### **Endpoint Information**

| **Method** | **Path** | **Description** |
|-------------|-----------|------------------|
| **DELETE** | `/strings/{string_value}` | Deletes the analysis by string content. |

---

### **Success Response** (`204 No Content`)

(Empty response body)

---

### **Error Responses**

| **Status** | **Error Description** |
|-------------|------------------------|
| **404 Not Found** | String does not exist in the system. |

