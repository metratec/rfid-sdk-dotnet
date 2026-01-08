# Changelog

## 3.4.1

* nn error that occurred during the preparation process when reconnecting has been fixed

## 3.4

* use .Net 8
* antenna parsing for multiple inventory corrected
* AT UHF Reader - GetInventory method splittet into GetSingleInventory and GetMultipleInventory
* Event timestamps corrected
* Parsing bugs adjusted
* Error messages updated
* Examples updated

## 3.4 Beta 2

* UHF inventory settings updated (Phase, Select, Target, RssiThreshold added)
* UHF regions added
* Input handling reworked
* Constructors and descriptions updated

## 3.4 Beta 1

* DwargG2 Reader and variations added
* timeouts for multiple inventory increased
* updated input handling for ascii readers
* disable the heartbeat feature when using a serial connection
* Updated constructors, description and error messages

## 3.3

* NFC Reader added
* Exceptions reworked (A distinction is now made between exceptions for readers and exceptions for transponders)

## 3.2

* set/get command method now public - for sending special custom command

## 3.1.1

* uhf gen2 inventory settings updated (FastStart added)

## 3.1.0

* Pulsar LR special commands added (SetRfMode, SetCustomImpinjSettings, CallImpinjAuthenticationService, SetSession)

## 3.0.0

Metratec rfid device library. For communication and control of Metratec RFID devices.
