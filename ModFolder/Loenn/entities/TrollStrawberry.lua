local strawberry = require("utils").deepcopy(require("entities.strawberry"))

strawberry.name = "ChroniaHelper/trollStrawberry"

for _, placement in pairs(strawberry.placements) do
    placement.data.reappear = false
    placement.data.spriteId = "strawberry"
    placement.name = "Troll Strawberry" .. " (" .. placement.name .. ")"
end

return strawberry