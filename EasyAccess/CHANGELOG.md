# Easy Access Change Log

## 2.2.0 (Unreleased)

### Added

* Added support for moss.

### Changed

* Updated for FauxCore 1.2.0.
* If config file is missing, it will attempt to restore from global data.

## 2.1.4 (April 12, 2024)

### Changed

* Initialize EasyAccess DI container on Entry.

### Fixed

* Fixed machines taking an extra item when dispensing.

## 2.1.3 (April 9, 2024)

### Changed

* Updated for FauxCore api changes.

## 2.1.2 (March 25, 2024)

### Fixed

* Fixed api integration with Toolbar Icons.

## 2.1.1 (March 19, 2024)

### Changed

* Rebuild against final SDV 1.6 and SMAPI 4.0.0.

## 2.1.0 (March 19, 2024)

### Changed

* Updated for SDV 1.6 and .NET 6

## 2.0.1 (July 9, 2022)

### Added

* Added alert if FuryCore is installed.
* Log config options for debugging.

## 2.0.0 (July 1, 2022)

* Added support for collecting foraging.
* Added support for shaking trees and bushes.
* Added support for collecting from dig spots.
* Updated to SMAPI 3.15.0.
* Removed dependency on FuryCore.

### Changed

* Added support for ModManifestBuilder.

## 1.2.2 (March 25, 2022)

* Updated to FuryCore 1.6.3.

## 1.2.1 (February 26, 2022)

* Updated to FuryCore 1.6.1.

## 1.2.0 (February 25, 2022)

* Updated to FuryCore 1.6.0.

## 1.1.0 (February 22, 2022)

* Updated to FuryCore 1.5.0.

### Added

* Integrate Configurator using new FuryCore service.

### Fixed

* DispenseInputs uses DispenseInputsPriority.

### Changed

* Allow producer name override from mod data.
* Producers are now sorted in GMCM.

## 1.0.1 (February 16, 2022)

* Updated to FuryCore 1.4.1

## 1.0.0 (February 15, 2022)

### Added

* Added toolbar icons for Collect Items and Dispense Items.
* Added support for more producers.

### Fixed

* Fixed some mishandled producers.

### Changed

* Purge inaccessible cached objects.

## 1.0.0-beta.1 (February 12, 2022)

* Initial Version