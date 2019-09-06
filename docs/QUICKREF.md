# Quick Reference

## Lookup an Entry

    !<keyword>

## Simple Entries

    !<keyword> = <description text>

## Simple Properties

    !<keyword>.<property> = <value>

## Removing Properties

    !<keyword>.<property> = null

## Removing Entries

    !<keyword> = null

## Using JSON

```
!<keyword> = ``​`json
{
  "title": "some title",
  "url": null,
  "description": "some text content",
  "Custom Field": "some text content"
}``​`

```
## Using YAML

```
!<keyword> = ``​`yaml
title: "some title"
url: null
description: "some text content"
"Custom Field": "some text content"
``​`
```

## Locking Entries

```
!<keyword>.locked = true
```

```
!<keyword>.locked = false
```