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

Entry content is comprised of a number of properties:

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
  <img src="./template.png">
</p>


