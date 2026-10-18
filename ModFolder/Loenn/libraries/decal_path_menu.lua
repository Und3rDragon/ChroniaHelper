local mods = require("mods")
local menubar = require("ui.menubar")
local configs = require("configs")
local tools = require("tools")
local keyboardHelper = require("utils.keyboard")

local requireFromPlugin = mods.requireFromPlugin

local decalBuilder = requireFromPlugin("libraries.decal_path_converter")

local library = {}

-- Menu entry keys, shared by the language file and the injected checkbox.
local MENU_KEY = "ChroniaHelper_convertDecalPaths"

-- Reads and writes the persisted switch state.
local MOD_SETTINGS_NAME = "ChroniaHelper"

local modSettings = mods.getModSettings(MOD_SETTINGS_NAME)

local INSTALL_FLAG = "keypressed_hooked_by_ChroniaHelperDecalPaths"

local installed = false


local function isEnabled()
    return modSettings.convertDecalPathsToDecals == true
end


--[[
    Reads the layer the selection tool is currently set to.

    The tool stores its layer directly on the tool table. It can also be the
    "all layers" table, in which case the background decals layer is used.
]]
local function getSelectedLayer()
    local selectionTool = tools.tools and tools.tools["selection"]

    if type(selectionTool) ~= "table" then
        return nil
    end

    local layer = selectionTool.layer

    if type(layer) == "string" then
        return layer
    end

    return nil
end


--[[
    Converts a pasted atlas image path into decal text, but only while the
    "Convert Decal Paths To Decals" switch is on.

    When the clipboard holds one or more image paths, the generated table is put
    back on the clipboard so the editor's own paste logic can place it. Anything
    else is left untouched, which keeps normal copy and paste working.
]]
local function tryConvertClipboard()
    if not isEnabled() then
        return false
    end

    local ok, clipboard = pcall(love.system.getClipboardText)

    if not ok or type(clipboard) ~= "string" or clipboard == "" then
        return false
    end

    local layer = getSelectedLayer()
    local serialized, count = decalBuilder.buildFromClipboardText(clipboard, layer)

    if not serialized then
        return false
    end

    return pcall(love.system.setClipboardText, serialized)
end


--[[
    True when the pressed key matches the configured paste activator, so unrelated
    shortcuts that share the same key are left alone.
]]
local function isPasteActivator(key)
    local activator = configs.hotkeys and configs.hotkeys.itemsPaste

    if not activator then
        return false
    end

    return keyboardHelper.activatorModifierString(key) == activator
end


--[[
    The editor only accepts clipboard text shaped like a table, so a pasted file
    path would otherwise be discarded.

    Wrapping the key handler runs before the editor acts on the paste, which lets
    the clipboard be rewritten in place when it holds image paths. Both the
    public handler and the table the main loop dispatches through are covered, so
    the wrapper is reached regardless of which one the engine calls.
]]
local function makeKeyHandler(originalKeypressed)
    return function(key, scancode, isrepeat)
        if not isrepeat and isEnabled() then
            local ok, matches = pcall(isPasteActivator, key)

            if ok and matches then
                pcall(tryConvertClipboard)
            end
        end

        return originalKeypressed(key, scancode, isrepeat)
    end
end


local function installKeyHandler()
    if installed then
        return true
    end

    if rawget(love, INSTALL_FLAG) then
        installed = true

        return true
    end

    local originalKeypressed = love.keypressed

    if type(originalKeypressed) ~= "function" then
        return false
    end

    rawset(love, INSTALL_FLAG, true)

    love.keypressed = makeKeyHandler(originalKeypressed)

    local handlers = love.handlers

    if type(handlers) == "table" and type(handlers.keypressed) == "function"
        and handlers.keypressed ~= originalKeypressed then
        handlers.keypressed = makeKeyHandler(handlers.keypressed)
    end

    installed = true

    return true
end


--[[
    Adds the "Convert Decal Paths To Decals" checkbox to the Edit menu.

    The switch starts disabled, so plain copy and paste keeps working until it is
    turned on from the menu.
]]
local function injectMenu()
    local editMenu = nil

    for _, menu in ipairs(menubar.menubar) do
        if menu[1] == "edit" then
            editMenu = menu[2]
            break
        end
    end

    if not editMenu then
        return
    end

    for _, item in ipairs(editMenu) do
        if item[1] == MENU_KEY then
            return
        end
    end

    table.insert(editMenu, {})

    table.insert(editMenu, {
        MENU_KEY,
        function()
            modSettings.convertDecalPathsToDecals = not isEnabled()
        end,
        "checkbox",
        function()
            return isEnabled()
        end,
    })
end


if not rawget(menubar, "ChroniaHelper_HaveAddDecalPathMenu") then
    rawset(menubar, "ChroniaHelper_HaveAddDecalPathMenu", true)
    injectMenu()
end

installKeyHandler()

return library
