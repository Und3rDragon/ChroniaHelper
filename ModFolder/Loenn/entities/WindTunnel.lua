local windTunnel = {}

local directions = {"Up", "Down", "Left", "Right"}
windTunnel.name = "ChroniaHelper/WindTunnel"
windTunnel.minimumSize = {16, 16}

windTunnel.fieldInformation = {
    strength = {
        minimumValue = 0.01,
    },
    direction = {
        editable = true,
        options = directions,
        allowEmpty = false,
    },
    flagMode = {
        options = {
            "AlwaysOn",
            "FlagNeeded",
            "FlagInverted",
        },
        editable = false,
    },
    particleColors = {
        fieldType = "list",
        elementOptions = {
            fieldType = "color",
        },
    }
}

windTunnel.placements = {}
for _, direction in ipairs(directions) do
    local activePlacement = {
        name = "Wind Tunnel (" .. direction .. ")",
        data = {
            width = 16,
            height = 16,
            direction = direction,
            flag = "Flag",
            strength = 100.0,
            particleDensity = 1.0,
            -- update: attribute mismatch
            particleStrengthOverride = -1,
            affectPlayer = true,
            particleColors  = "808080,545151,ada5a5",
            showParticles = true,
            flagMode = "AlwaysOn",
        }
    }
    table.insert(windTunnel.placements, activePlacement)
end

windTunnel.fillColor = {0.7, 0.7, 0.7, 0.4}
windTunnel.borderColor = {0.7, 0.7, 0.7, 1.0}

return windTunnel