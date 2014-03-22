sonos-auto-bookmarker
=====================

A small tool that can run in a household constantly monitoring the SONOS systems on what is being played. Whenever an audiobook or podcast starts playing it stores the current progress of listening / play position. When playing is resumed (after playing other tracks) this tool will seek to the last known play position of that track.

Pre-Requisites
===============

You will need to have a running https://github.com/jishi/node-sonos-http-api instance (requires NodeJS). To run the tool you need mono.

When you compile and run you can change the NodeJS URL through the configuration.json file.
