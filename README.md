sonos-auto-bookmarker
=====================

A small tool that can run in a household constantly monitoring the SONOS systems on what is being played. Whenever an audiobook or podcast starts playing it stores the current progress of listening / play position. When playing is resumed (after playing other tracks) this tool will seek to the last known play position of that track.

Pre-Requisites
===============

You will need to have a running https://github.com/jishi/node-sonos-http-api instance (requires NodeJS). To run the tool you need mono.

When you compile and run you can change the NodeJS URL through the configuration.json file.

Configuration
===============

The sonos-auto-bookmarker takes the configuration.json file as input. It looks like this:

    {
	  "SONOS_HTTP_API_URL":"http://192.168.178.96:5005",
	  "UpdateIntervalSeconds":1,
	  "BookmarkOnlyLongerThanSeconds":600,
	  "MinimalSecondsPerSave":60,
	  "MinimalChangesPerSave":1,
	  "Bookmarks":[],
	  "IgnoreNamePattern":["Whisky Tastings"] 
	}
	
As the keywords have the following meaning:

- SONOS_HTTP_API_URL
 - the URL under which the NodeJS SONOS HTTP API is running/hosted.
- UpdateIntervalSeconds
 - the time in seconds in which the SONOS API is called for new information.
- BookmarkOnlyLongerThanSeconds
 - only tracks longer than this (in second) will be considered for bookmarking
- MinimalSecondPerSave
 - how many seconds must have passed since the last write to disk / saveing of the configuration file to write again (to save write operations)
- MinimalChangesPerSave
 - how many changes / new bookmarks need to have been added to be written to disk
- Bookmarks
 - the bookmarks themselves - this will be filled over time by the sonos-auto-bookmarker tool
- IgnoreNamePattern
 - an array of .NET regular expression patterns that, when they match the track title, will lead to sonos-auto-bookmarker ignoring that track
