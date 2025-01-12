using CustomAvatarLoader.Modules;
using Il2Cpp;
using Il2CppUniGLTF;
using Il2CppUniVRM10;
using UnityEngine;
using ILogger = CustomAvatarLoader.Logging.ILogger;

namespace CustomAvatarLoader.Chara;

public class CharaLoader
{
    private ILogger _logger;
    private CharaData CharaData { get; set; }

    private RuntimeAnimatorController RuntimeAnimatorController { get; set; }

    public IList<IModule> Modules { get; } = new List<IModule>();

    public CharaLoader(ILogger logger)
    {
        _logger = logger;
    }
    
    public bool LoadCharacter(string path)
    {
        if (!File.Exists(path))
        {
            _logger.Error("VRM file does not exist: " + path);
            return false;
        }

        GameObject newChara;
        try
        {
            var data = new GlbFileParser(path).Parse();
            var vrmdata = Vrm10Data.Parse(data);
            if (vrmdata == null)
            {
                MigrationData migrationData;
                Vrm10Data.Migrate(data, out vrmdata, out migrationData);
                if (vrmdata == null)
                {
                    throw new Exception("Cannot load vrm file!");
                }
            }

            var context = new Vrm10Importer(vrmdata);
            var loaded = context.Load();

            loaded.EnableUpdateWhenOffscreen();
            loaded.ShowMeshes();
            loaded.gameObject.name = "VRMFILE";
            newChara = loaded.gameObject;
        }
        catch (Exception ex)
        {
            _logger.Error("Error trying to load the VRM file!", ex);
            return false;
        }

        var chara = GameObject.Find("/CharactersRoot").transform.GetChild(0).gameObject;
        CharaData = chara.GetComponent<CharaData>();
        RuntimeAnimatorController = chara.GetComponent<Animator>().runtimeAnimatorController;

        _logger.Debug("Chara copied! Removing default chara...");
        UnityEngine.Object.Destroy(chara);

        newChara.transform.parent = GameObject.Find("/CharactersRoot").transform;

        CharaData newCharaData = newChara.AddComponent<CharaData>();
        CopyCharaData(CharaData, newCharaData);

        MainManager manager = GameObject.Find("MainManager").GetComponent<MainManager>();
        manager.charaData = newCharaData;

        Animator charaAnimator = newChara.GetComponent<Animator>();
        charaAnimator.applyRootMotion = true;
        charaAnimator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        charaAnimator.runtimeAnimatorController = RuntimeAnimatorController;

        _logger.Debug("Chara replaced!");

        return true;
    }
        
    private void CopyCharaData(CharaData source, CharaData target)
    {
        target.alarmAnim = source.alarmAnim;
        target.draggedAnims = source.draggedAnims;
        target.hideLeftAnims = source.hideLeftAnims;
        target.hideRightAnims = source.hideRightAnims;
        target.jumpInAnim = source.jumpInAnim;
        target.jumpOutAnim = source.jumpOutAnim;
        target.pickedSittingAnim = source.pickedSittingAnim;
        target.pickedStandingAnim = source.pickedStandingAnim;
        target.sittingOneShotAnims = source.sittingOneShotAnims;
        target.sittingRandomAnims = source.sittingRandomAnims;
        target.standingOneShotAnims = source.standingOneShotAnims;
        target.standingRandomAnims = source.standingRandomAnims;
        target.strokedSittingAnim = source.strokedSittingAnim;
        target.strokedStandingAnim = source.strokedStandingAnim;
    }
}