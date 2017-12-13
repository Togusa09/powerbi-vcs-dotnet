# Metadata

## File Structure
|Offset|Size|Type|Description|
|------|----|----|-----------|
|0|8|Byte|Initial offset, unknown purpose|
|8|4|INT32|Number of records in section 1|
| | |Each record is composed of two strings, a name/key pair|
| |4|INT32|Number of records in section 2|
| | |Each record is composed of two strings, a key/name pair|
| |1|byte|Unknown purpose|
| |1|byte/number|Number of records in section 3 
| | |Section 3 - List of strings|

## Strings
|Offset|Size|Type|Description|
|------|----|----|-----------|
|0|1|Byte|Number of characters in the string|
|1|N|Char[]|String data|

# Data Mashup

## File Structure
|Offset|Size|Type|Description|
|------|----|----|-----------|
|0|4|Byte|Null bytes, unknown purpose|
|4|4|Int32|Length of zip file|
|8|N|Zip file|Binary data for zip file of length stated above|
| |4|Int32|Length of first xml block|
| |N|char[]|UTF-8 xml file of above length|
| |4|Int32|Length of xml block 2 + 34|
| |4|byte|Null bytes, unknown purpose|
| |4|Int32|Length of xml block 2|
| |N|char[]|XML block|
| | |byte|Unknown|