![Version](https://img.shields.io/badge/Version-2.1.2-orange)
[![GitHub issues](https://img.shields.io/github/issues/cheeeeeeeeeen/RoR2-Qb-Drone)](https://github.com/cheeeeeeeeeen/RoR2-Qb-Drone/issues)
[![GitHub pull requests](https://img.shields.io/github/issues-pr/cheeeeeeeeeen/RoR2-Qb-Drone)](https://github.com/cheeeeeeeeeen/RoR2-Qb-Drone/pulls)
[![Support Chen](https://img.shields.io/badge/Support-Chen-ff69b4)](https://ko-fi.com/cheeeeeeeeeen)
![Maintenance Status](https://img.shields.io/badge/Maintenance-Inactive-orange)

# **Qb** Drone

## Description

A sample, fully functional drone added to Risk of Rain 2.

The drone is powered by the API of **[Chen's Gradius Mod](https://github.com/cheeeeeeeeeen/RoR2-ChensGradiusMod)**.

#### What is this?

A very mysterious drone of unknown origins. It is literally a cube that floats.

There is a model name and number on an edge that says...

**Qb.729.216.8 - "Cute Cube"**.

That is all we know about it.

#### Actions

**Lob Grenades** - It seems to form grenades from one of its central nodes and lobs them. The grenades are armed, so stay clear out of it.

**Close-Proximity Bomb Thing** - It seems to shoot fireballs that explode on contact. It only does so to nearby hostiles with uncanny accuracy.

**Mayhem Salvo** - It has a weird behavior of generating rockets, missiles and mines combined at such a fast rate, all coming out from all of its nodes. This lasts for quite some time. The immense firepower is impressive.

## Installation

Use **[Thunderstore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager)** to install this mod.

If one does not want to use a mod manager, then get the DLL from **[Thunderstore](https://thunderstore.io/package/Chen/Qb/)**.

## Contact
- Issue Page: https://github.com/cheeeeeeeeeen/RoR2-Qb-Drone/issues
- Email: `blancfaye7@gmail.com`
- Discord: `Chen#1218`
- RoR2 Modding Server: https://discord.com/invite/5MbXZvd
- Give a tip through Ko-fi: https://ko-fi.com/cheeeeeeeeeen

## Changelog

**2.1.2**
- Recompile to update libraries.

**2.1.1**
- Migrate to nuget packages.
- Update code so that it follows the new DirectorAPI syntax.

**2.0.11**
- Optimize resource loading.
- Apply HGStandard Shaders to Qb. Should look good now.
- Implement Aim Origins from each node. This should properly spawn some projectiles as well as muzzle effects.

**2.0.10**
- Fix a bug where the Qb Drone seems to duplicate interactables when it is decommissioned.
- Add broken effects to the Qb Drone.
- Update the code so that it works after the update of Chen's Helpers and Chen's Gradius Mod.

**2.0.9**
- Update code so that it works after the breaking changes of Chen's Helpers.
- Integrate shortcut methods from Chen's Gradius Mod.

**2.0.8**
- Update the mod so that it works after the update of Chen's Gradius Mod.
- Optimize by caching all resource loading.

**2.0.7**
- Double the projectile speed of Close-Proximity Bomb Thing, as well as decrease the range of detection.
- Integrate Content Pack management from Chen's Helpers.

**2.0.6**
- Add Arms Race support from Chen's Classic Items.
- Update the code so that Qb will follow new rules of Artifact of Machines from Chen's Gradius Mod.

**2.0.5**
- Update the mod so that it works after another update of Chen's Gradius Mod.
- Actually add Chen's Gradius Mod as a BepInDependency. Doh.

**2.0.4**
- Update the mod so that it works after the update of Chen's Gradius Mod.
- Implement a lifetime for Qb's Spider Mines so that it self destructs (or rather disappear) after a period of time.

**2.0.3**
- Update the code so that it works with the latest R2API.

**2.0.2**
- Integrate latest changes from ChensHelpers.

**2.0.1**
- Fix the hard crash caused by a Null Reference Exception.

**2.0.0**
- Update the mod so that it works with anniversary update.

**1.0.1**
- Implement Option integration for all actions of Qb.

**1.0.0**
- Initial release.