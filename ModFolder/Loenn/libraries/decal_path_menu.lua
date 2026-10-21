local mods = require("mods")
local menubar = require("ui.menubar")
local configs = require("configs")
local tools = require("tools")
local keyboardHelper = require("utils.keyboard")

local requireFromPlugin = mods.requireFromPlugin

local jumpThruBuilder = requireFromPlugin("libraries.jumpthru_path_converter")
local decalBuilder = requireFromPlugin("libraries.decal_path_converter")
local spinnerBuilder = requireFromPlugin("libraries.spinner_path_converter")
local boosterBuilder = requireFromPlugin("libraries.booster_path_converter")

local library = {}

-- Menu entry keys, shared by the language file and the injected checkboxes.
local GROUP_MENU_KEY = "ChroniaHelper_convertPastedPaths"
local JUMPTHRU_MENU_KEY = "ChroniaHelper_convertJumpThruPaths"
local DECAL_MENU_KEY = "ChroniaHelper_convertDecalPaths"
local ONE_ONLY_MENU_KEY = "ChroniaHelper_enableOneOnly"
local SPINNER_MENU_KEY = "ChroniaHelper_convertSpinnerPaths"
local BOOSTER_MENU_KEY = "ChroniaHelper_convertBoosterPaths"

-- Reads and writes the persisted switch states.
local MOD_SETTINGS_NAME = "ChroniaHelper"

local modSettings = mods.getModSettings(MOD_SETTINGS_NAME)

local JUMPTHRU_SETTINGS_KEY = "convertJumpThruPathsToJumpThrus"
local DECAL_SETTINGS_KEY = "convertDecalPathsToDecals"
local SPINNER_SETTINGS_KEY = "convertSpinnerPathsToSpinners"
local BOOSTER_SETTINGS_KEY = "convertBoosterPathsToBoosters"

--[[
    Every converter starts disabled, so plain copy and paste keeps working until
    one of them is switched on from the Edit menu. The defaults are written once
    so the stored state stays explicit instead of relying on missing keys.
]]
local function initSetting(key)
    if modSettings[key] == nil then
        modSettings[key] = false
    end
end

initSetting(JUMPTHRU_SETTINGS_KEY)
initSetting(DECAL_SETTINGS_KEY)
initSetting(SPINNER_SETTINGS_KEY)
initSetting(BOOSTER_SETTINGS_KEY)

local INSTALL_FLAG = "keypressed_hooked_by_ChroniaHelperDecalPaths"

local installed = false


local function isJumpThruEnabled()
    return modSettings[JUMPTHRU_SETTINGS_KEY] == true
end


local function isDecalEnabled()
    return modSettings[DECAL_SETTINGS_KEY] == true
end


local function isSpinnerEnabled()
    return modSettings[SPINNER_SETTINGS_KEY] == true
end


local function isBoosterEnabled()
    return modSettings[BOOSTER_SETTINGS_KEY] == true
end


local function isAnyEnabled()
    return isJumpThruEnabled() or isDecalEnabled() or isSpinnerEnabled() or isBoosterEnabled()
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
    Converters in the order they are tried.

    Path matching is more specific for jump throughs and decals, so those are
    checked first. Spinners and boosters both accept any gameplay texture path,
    which is why they come last and settle the remaining paths between them.
]]
local function getConverters()
    local converters = {}

    if isJumpThruEnabled() then
        table.insert(converters, function(clipboard)
            return jumpThruBuilder.buildFromClipboardText(clipboard)
        end)
    end

    if isDecalEnabled() then
        table.insert(converters, function(clipboard)
            return decalBuilder.buildFromClipboardText(clipboard, getSelectedLayer())
        end)
    end

    if isSpinnerEnabled() then
        table.insert(converters, function(clipboard)
            return spinnerBuilder.buildFromClipboardText(clipboard)
        end)
    end

    if isBoosterEnabled() then
        table.insert(converters, function(clipboard)
            return boosterBuilder.buildFromClipboardText(clipboard)
        end)
    end

    return converters
end


--[[
    Converts a pasted texture path into editable data using the first converter
    that recognises it.

    The generated table is put back on the clipboard so the editor's own paste
    logic can place it. Paths no enabled converter recognises are left untouched,
    which keeps normal copy and paste working.
]]
local function tryConvertClipboard()
    local converters = getConverters()

    if #converters == 0 then
        return false
    end

    local ok, clipboard = pcall(love.system.getClipboardText)

    if not ok or type(clipboard) ~= "string" or clipboard == "" then
        return false
    end

    for _, convert in ipairs(converters) do
        local convertOk, serialized = pcall(convert, clipboard)

        if convertOk and serialized then
            return pcall(love.system.setClipboardText, serialized)
        end
    end

    return false
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
    the clipboard be rewritten in place. Both the public handler and the table the
    main loop dispatches through are covered, so the wrapper is reached regardless
    of which one the engine calls.
]]
local function makeKeyHandler(originalKeypressed)
    return function(key, scancode, isrepeat)
        if not isrepeat and isAnyEnabled() then
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


local function hasItem(menu, langKey)
    for _, item in ipairs(menu) do
        if item[1] == langKey then
            return true
        end
    end

    return false
end


local function insertCheckbox(menu, langKey, toggle, active)
    table.insert(menu, {
        langKey,
        toggle,
        "checkbox",
        active,
    })
end


--[[
    Adds a checkbox for a converter to the given menu, unless it is already there.
]]
local function addConverterCheckbox(menu, langKey, settingsKey)
    if hasItem(menu, langKey) then
        return
    end

    insertCheckbox(menu, langKey,
        function()
            modSettings[settingsKey] = not (modSettings[settingsKey] == true)
        end,
        function()
            return modSettings[settingsKey] == true
        end)
end


--[[
    Builds the submenu holding the converter switches.

    Spinners and boosters accept any gameplay texture path, so they are grouped
    one level deeper to keep them away from the specific converters.
]]
local function buildConverterGroup()
    local group = {}

    addConverterCheckbox(group, JUMPTHRU_MENU_KEY, JUMPTHRU_SETTINGS_KEY)
    addConverterCheckbox(group, DECAL_MENU_KEY, DECAL_SETTINGS_KEY)

    local oneOnly = {}

    addConverterCheckbox(oneOnly, SPINNER_MENU_KEY, SPINNER_SETTINGS_KEY)
    addConverterCheckbox(oneOnly, BOOSTER_MENU_KEY, BOOSTER_SETTINGS_KEY)

    table.insert(group, { ONE_ONLY_MENU_KEY, oneOnly })

    return group
end


--[[
    Adds the "Convert Pasted Paths" submenu to the Edit menu.

    Every converter starts disabled, so plain copy and paste keeps working until
    one of them is turned on.
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

    if hasItem(editMenu, GROUP_MENU_KEY) then
        return
    end

    table.insert(editMenu, {})
    table.insert(editMenu, { GROUP_MENU_KEY, buildConverterGroup() })
end


if not rawget(menubar, "ChroniaHelper_HaveAddDecalPathMenu") then
    rawset(menubar, "ChroniaHelper_HaveAddDecalPathMenu", true)
    injectMenu()
end

installKeyHandler()

return library
