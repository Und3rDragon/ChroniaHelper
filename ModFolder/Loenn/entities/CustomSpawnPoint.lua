local customSpawnPoint = {}

customSpawnPoint.name = "ChroniaHelper/CustomSpawnPoint"
customSpawnPoint.depth = 0
customSpawnPoint.justification = {0.5, 1.0}
customSpawnPoint.texture = "characters/player/sitDown00"
customSpawnPoint.placements = {
    name = "normal",
    data = {
        isDefaultSpawn = false,
        cameraOffsetX = 0,
        cameraOffsetY = 0,
        setFlag = "",
        playerFacingRight = true,
    }
}

return customSpawnPoint
