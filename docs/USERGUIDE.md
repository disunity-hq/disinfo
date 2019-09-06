# Disinfo User-guide

Disinfo acts as a database for embed messages. 

Embeds are useful for sharing a collection of information in a nice little package. With Disinfo, you can store information inside of Embeds and recall them later by Keyword.


# Entries

Each Embed is stored as an Entry within the database. This section will explain how to work with Entries.

## Keywords and Embed Content

Each entry in the database is made of two parts. The **Keyword** and the **Embed Content**.

- **Keyword** - acts as the name for an entry
- **Embed Content** - the information inside the embed

### Keywords

When creating an entry, the Keyword can be any text content. As it is saved to the database it will be "slugified":

- Spaces transformed into dashes `-`
- Non-alphanumeric characters are removed
- Everything is lower-cased

This produces Keywords like `important-info` or `some-random-topic`.

When querying for an entry, it is this slugified form of the Keyword which must be used.

### Embed Content

Each entry must have some content, otherwise it is removed from the database. 

Entry content is comprised of a number of properties. Any properties that are not in the following table are saved as custom fields.

| Property    | Description                            |
|-------------|----------------------------------------|
| Description | The main textual content               |
| Title       | The title of the entry                 |
| Url         | Turns the title into a link            |
| Author      | Displayed at the very top of the embed |
| Image       | URL of an image to display             |
| Thumbnail   | URL of a thumbnail to display          |
| Color       | Color of the embed's side-margin       |

<hr style="opacity: 0">
<p align="center">
  <img src="./docs/imgs/template.png">
</p>

## Adding Simple Entries

A simple "description only" entry can be added:

<p align="center">
  <img src="./docs/imgs/simple-entry.png">
</p>

## Setting individual properties

Individual properties can be set:

<p align="center">
  <img src="./docs/imgs/setting-properties.png">
</p>

## Setting multiple properties

Multiple properties can be set at once:

<p align="center">
  <img src="./docs/imgs/multiple-properties.png">
</p>

## Removing properties

Remove properties by setting them to null. Multiple properties can be removed this way.

<p align="center">
  <img src="./docs/imgs/removing-properties.png">
</p>

## Removing entries

Entire entries can be removed by setting their Keyword to null:

<p align="center">
  <img src="./docs/imgs/removing-entries.png">
</p>


## Using JSON

Entire entries can be created at once by utilizing a JSON object within triple-backticks. You must specify the format directly after the opening triple-backticks. The JSON content must follow on the next line and be well-formed:

<p align="center">
  <img src="./docs/imgs/using-json.png">
</p>

## Using YAML

Entries can also be created by utilizing a YAML object within triple-backticks. You must specify the format directly after the opening triple-backticks. The YAML content must follow on the next line and be well-formed:

<p align="center">
  <img src="./docs/imgs/using-yaml.png">
</p>

# Locked Entries

When Disinfo is invited and joins a server, a Role that shares its name is automatically created. Only users with a Role above the bot Role can manage Locked Entries.

To lock an Entry simply set its `.locked` property to `true`. Notice the indication that the entry is now locked in the footer of the Embed:

<p align="center">
  <img src="./docs/imgs/locked-entries.png">
</p>

Users without a sufficient Role wont be allowed to modify Locked Entries:

<p align="center">
  <img src="./docs/imgs/locked-entries.png">
</p>

They can still query for them though:

<p align="center">
  <img src="./docs/imgs/locked-query.png">
</p>


# Local and Global Entries

There are two kinds of Entries, **Local** and **Global**.


## Local Entries

Disinfo maintains a completely isolated set of entries for each server on which it resides. Entries created on one server cannot be seen on any other server.

## Global Entries

Disinfo's bot owner can maintain a set of Global Entries which are visible on every server. Only the owner can manipulate the Global entries.

Local entries cannot be created with the same Keyword as a Global Entry.

### Managing Global Entries

Global Entries are managed by the bot owner through DM. All of the commands are exactly the same, except they work against the Global Entries.

