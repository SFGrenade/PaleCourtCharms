using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using SFCore.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vasi;
using Random = UnityEngine.Random;

namespace PaleCourtCharms
{
    internal class LamentControl : MonoBehaviour
    {
        public static List<GameObject> markedEnemies = new List<GameObject>();
        private PlayMakerFSM _spellControl;
        private PlayerData _pd = PlayerData.instance;
        private HeroController _hc = HeroController.instance;
        private PlayMakerFSM _pvControl;

        private void OnEnable()
        {
            On.DamageEnemies.FixedUpdate += SuppressDamageEnemiesError;
            On.HealthManager.TakeDamage += ApplyStatus;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ClearList;

            _pvControl = _hc.gameObject.Find("HK Prime(Clone)(Clone)").LocateMyFSM("Control");

            _spellControl = HeroController.instance.spellControl;
            if (_spellControl != null)
            {
                _spellControl.InsertMethod("Cancel All", 33, BlastControlCancel);
                _spellControl.InsertMethod("Focus Cancel", 15, BlastControlCancel);
                _spellControl.InsertMethod("Focus Cancel 2", 18, BlastControlCancel);

                _spellControl.InsertMethod("Focus Blast", 15, BlastControlFadeIn);
                _spellControl.InsertMethod("Start MP Drain Blast", 2, BlastControlFadeIn);

                _spellControl.InsertMethod("Focus Heal Blast", 16, BlastControlMain);
                _spellControl.InsertMethod("Focus Heal 2 Blast", 18, BlastControlMain);
            }

            foreach (var pool in ObjectPool.instance.startupPools)
            {
                if (pool.prefab.name == "Knight Spore Cloud")
                {
                    PaleCourtCharms.preloadedGO["SporeCloud"] = pool.prefab;
                }

                if (pool.prefab.name == "Knight Dung Cloud")
                {
                    PaleCourtCharms.preloadedGO["DungCloud"] = pool.prefab;
                }
            }
        }

        private void BlastControlCancel()
        {
            PureVesselBlastCancel();
        }

        private void PureVesselBlastCancel()
        {
            foreach (GameObject enemy in markedEnemies)
            {
                var index = markedEnemies.IndexOf(enemy);

                try
                {
                    if (enemy.GetComponent<Afflicted>()._focusLines != null)
                    {
                        enemy.GetComponent<Afflicted>()._focusLines.GetComponent<tk2dSpriteAnimator>().Stop();

                        Destroy(enemy.GetComponent<Afflicted>()._focusLines);
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                }

                try
                {
                    if (markedEnemies[index] != null)
                    {
                        if (markedEnemies[index].GetComponent<Afflicted>() == null)
                        {
                            Log("Afficted component not found");
                        }
                        else
                        {
                            try
                            {
                                enemy.GetComponent<Afflicted>().visible = true;
                                enemy.GetComponent<Afflicted>().SoulEffect.SetActive(true);
                                enemy.GetComponent<Afflicted>().SoulEffect.GetComponent<ParticleSystem>().Play();
                            }
                            catch (NullReferenceException e)
                            {
                            }

                            ;
                        }
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Log("Exception caught in soul effect");
                }

                try
                {
                    enemy.GetComponent<Afflicted>().StopCoroutine(enemy.GetComponent<Afflicted>()._createLine);
                }
                catch (NullReferenceException e)
                {
                    Log("Couldn't stop create line couroutine");
                }

                try
                {
                    Destroy(enemy.GetComponent<Afflicted>()._blast);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Log("Exception caught in blast");
                }
                //_hc.gameObject.GetComponent<LamentControl>()._audio.Stop();
            }
        }

        private void BlastControlFadeIn()
        {
            for (int i = 0; i < markedEnemies.Count; i++)
            {
                GameObject enemy = markedEnemies[i];
                for (int j = 0; j < markedEnemies.Count; j++)
                {
                    GameObject compare = markedEnemies[j];
                    if (i == j) continue;
                    if (compare != enemy) continue;

                    Log("Removed Duplicate Object");
                    Log($"{compare} was in the list twice");
                    markedEnemies.Remove(compare);
                }


                if (enemy == null || !enemy.active)
                {
                    markedEnemies.RemoveAt(i);

                    i--;
                    continue;
                }

                enemy.GetComponent<Afflicted>().StartCoroutine(enemy.GetComponent<Afflicted>().PureVesselBlastFadeIn());
            }
        }

        private void BlastControlMain()
        {
            List<int> nullenemies = new List<int>();
            foreach (GameObject enemy in markedEnemies)
            {
                var index = markedEnemies.IndexOf(enemy);
                if (enemy == null || !enemy.active)
                {
                    nullenemies.Add(index);
                    Modding.Logger.LogFine("Item was null, continuing");
                    continue;
                }

                /*enemy.GetComponent<Afflicted>().*/
                StartCoroutine(enemy.GetComponent<Afflicted>().PureVesselBlast());
            }

            foreach (int i in nullenemies)
            {
                markedEnemies.RemoveAt(i);
            }
        }

        private void SuppressDamageEnemiesError(On.DamageEnemies.orig_FixedUpdate orig, DamageEnemies self)
        {
            try
            {
                orig(self);
            }
            catch (ArgumentOutOfRangeException)
            {
                // swallow the error so it no longer spams in the modlog
            }
        }

        private void OnDisable()
        {
            On.DamageEnemies.FixedUpdate -= SuppressDamageEnemiesError;
            On.HealthManager.TakeDamage -= ApplyStatus;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= ClearList;

            if (_spellControl != null)
            {
                _spellControl.RemoveAction("Cancel All", 33);
                _spellControl.RemoveAction("Focus Cancel", 15);
                _spellControl.RemoveAction("Focus Cancel 2", 18);

                _spellControl.RemoveAction("Focus Blast", 15);
                _spellControl.RemoveAction("Start MP Drain Blast", 2);

                _spellControl.RemoveAction("Focus Heal Blast", 16);
                _spellControl.RemoveAction("Focus Heal 2 Blast", 18);
            }
        }

        private void ClearList(Scene PrevScene, Scene NextScene)
        {
            markedEnemies.Clear();
            foreach (GameObject go in markedEnemies)
            {
                Log($"This should be empty but {go} is still there");
            }
        }

        private void ApplyStatus(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            orig(self, hitInstance);
            if (hitInstance.AttackType == AttackTypes.Nail)
            {
                if (self.gameObject.GetComponent<Afflicted>() == null)
                {
                    self.gameObject.AddComponent<Afflicted>();
                    markedEnemies.Add(self.gameObject);
                }
            }
        }


        private void Log(object message) => Modding.Logger.Log("[PC_Charms][LamentControl] " + message);
    }

    internal class Afflicted : MonoBehaviour
    {
        public GameObject SoulEffect;
        public GameObject _focusLines;
        public GameObject _line;
        public GameObject _blast;
        public IEnumerator _createLine;
        private PlayMakerFSM _pvControl;
        private HeroController _hc = HeroController.instance;
        private PlayerData _pd = PlayerData.instance;
        public bool visible;

        private void Start()
        {
            _pvControl = _hc.gameObject.Find("HK Prime(Clone)(Clone)").LocateMyFSM("Control");
            SoulEffect = Instantiate(PaleCourtCharms.preloadedGO["SoulEffect"], gameObject.transform);
            SoulEffect.transform.localPosition = new Vector3(0, 0, -0.0001f);
            Vector2 center = gameObject.transform.position;
            if (gameObject.GetComponent<SpriteRenderer>() != null)
            {
                center = gameObject.GetComponent<SpriteRenderer>().bounds.center;
            }

            if (gameObject.GetComponent<tk2dSprite>() != null)
            {
                center = gameObject.gameObject.GetComponent<tk2dSprite>().GetBounds().center + gameObject.transform.position;
            }

            SoulEffect.transform.position = center;
            SoulEffect.transform.localScale = new Vector3(.75f, .75f, .75f);

            SoulEffect.GetComponent<ParticleSystem>().startSize = .7f;
            SoulEffect.GetComponent<ParticleSystem>().startLifetime = .3f;
            SoulEffect.GetComponent<ParticleSystem>().startColor = new Color(1, 1, 1, .5f);

            SoulEffect.SetActive(true);
        }

        private void Update()
        {
            if (SoulEffect != null)
            {
                Vector3 center = gameObject.transform.position;
                if (gameObject.GetComponent<SpriteRenderer>() != null)
                {
                    center = gameObject.GetComponent<SpriteRenderer>().bounds.center;
                }

                if (gameObject.GetComponent<tk2dSprite>() != null)
                {
                    center = gameObject.GetComponent<tk2dSprite>().GetBounds().center + gameObject.transform.position;
                }

                SoulEffect.transform.position = new Vector3(center.x, center.y, gameObject.transform.position.z + -0.0001f);
                if (visible)
                {
                    if (gameObject.GetComponent<SpriteRenderer>() != null)
                    {
                        SoulEffect.SetActive(gameObject.GetComponent<SpriteRenderer>().isVisible);
                    }

                    if (gameObject.GetComponent<MeshRenderer>() != null)
                    {
                        SoulEffect.SetActive(gameObject.GetComponent<MeshRenderer>().isVisible);
                    }
                }
            }
            else
            {
                SoulEffect = new GameObject();
                Start();
            }

            if (_focusLines != null && _pd.GetBool("equippedCharm_" + Charms.ShapeOfUnn))
            {
                _focusLines.transform.position = gameObject.transform.position;
            }

            if (_blast != null && _pd.GetBool("equippedCharm_" + Charms.ShapeOfUnn))
            {
                _blast.transform.position = gameObject.transform.position;
            }
        }

        public IEnumerator FadeOut()
        {
            //while (SoulEffect.GetComponent<SpriteRenderer>().color.a > 0)
            //{
            //   yield return new WaitForSeconds(.01f);
            //   SoulEffect.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, SoulEffect.GetComponent<SpriteRenderer>().color.a - .1f);
            // }
            SoulEffect.GetComponent<ParticleSystem>().Stop();
            yield return new WaitUntil(() => SoulEffect.GetComponent<ParticleSystem>().isStopped);
            visible = false;
            SoulEffect.SetActive(false);
        }

        private void OnDisable()
        {
            Destroy(SoulEffect);
        }

        public IEnumerator PureVesselBlastFadeIn()
        {
            StartCoroutine(FadeOut());

            _createLine = CreateLine();
            StartCoroutine(_createLine);
            _focusLines = Instantiate(_hc.gameObject.Find("Focus Effects").Find("Lines Anim"), gameObject.transform.position, Quaternion.identity);
            _focusLines.GetComponent<tk2dSpriteAnimator>().Play("Focus Effect");

            this.PlayAudio((AudioClip)_pvControl.GetAction<AudioPlayerOneShotSingle>("Focus Charge", 2).audioClip.Value, 0, 1.5f);
            _blast = Instantiate(PaleCourtCharms.preloadedGO["Blast"]);
            _blast.transform.position += gameObject.transform.position;
            _blast.SetActive(true);
            Destroy(_blast.FindGameObjectInChildren("hero_damager"));

            if (_pd.GetBool("equippedCharm_" + Charms.DeepFocus))
            {
                _blast.transform.localScale *= 2.5f;
            }
            else
            {
                _blast.transform.localScale *= 1.5f;
            }

            Animator anim = _blast.GetComponent<Animator>();
            anim.speed = 1;
            if (_pd.GetBool("equippedCharm_" + Charms.QuickFocus))
            {
                anim.speed *= 1.33f;
            }

            if (_pd.GetBool("equippedCharm_" + Charms.DeepFocus))
            {
                anim.speed -= anim.speed * 0.35f;
            }

            yield return null;
        }

        public IEnumerator CreateLine()
        {
            var wait = 1f;
            if (_pd.GetBool("equippedCharm_" + Charms.QuickFocus))
            {
                wait /= 1.33f;
            }

            if (_pd.GetBool("equippedCharm_" + Charms.DeepFocus))
            {
                wait -= wait * 0.35f;
            }

            yield return new WaitForSeconds(wait - .2f);
            var heropos = _hc.transform.position - new Vector3(0, 1, 0);
            var enemypos = gameObject.transform.position;
            var linepos = Vector3.Lerp(heropos, enemypos, .5f);

            float num = heropos.y - enemypos.y;
            float num2 = heropos.x - enemypos.x;
            float lineangle;
            for (lineangle = Mathf.Atan2(num, num2) * (180f / (float)Math.PI); lineangle < 0f; lineangle += 360f)
            {
            }

            var linesize = Vector2.Distance(heropos, enemypos);

            _line = Instantiate(PaleCourtCharms.preloadedGO["SoulTwister"].LocateMyFSM("Mage").GetAction<CreateObject>("Tele Line").gameObject.Value, linepos, new Quaternion(0, 0, 0, 0));
            _line.transform.SetRotationZ(lineangle);
            _line.transform.localScale = new Vector3(linesize, 1, 1);
            // _line.GetComponent<ParticleSystem>().loop = true;
            _line.GetComponent<ParticleSystem>().startSize = .35f;
            _line.GetComponent<ParticleSystem>().emissionRate = 3000;
            _line.GetComponent<ParticleSystem>().startLifetime = .75f;
            _line.GetComponent<ParticleSystem>().Emit(0);
            _line.SetActive(true);
            _line.GetComponent<ParticleSystem>().Play();
            _line.GetComponent<ParticleSystem>().loop = false;
        }

        public IEnumerator PureVesselBlast()
        {
            _focusLines.GetComponent<tk2dSpriteAnimator>().Play("Focus Effect End");
            _blast.layer = 17;
            Animator anim = _blast.GetComponent<Animator>();
            anim.speed = 1;
            int hash = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
            anim.PlayInFixedTime(hash, -1, 0.8f);


            CircleCollider2D blastCollider = _blast.AddComponent<CircleCollider2D>();
            blastCollider.radius = 2.5f;
            if (_pd.GetBool("equippedCharm_" + Charms.DeepFocus))
            {
                blastCollider.radius *= 2.5f;
            }
            else
            {
                blastCollider.radius *= 1.5f;
            }

            blastCollider.offset = Vector3.down;
            blastCollider.isTrigger = true;


            _blast.AddComponent<DamageEnemies>();
            DamageEnemies damageEnemies = _blast.GetComponent<DamageEnemies>();
            damageEnemies.damageDealt = _pd.GetBool("equippedCharm_" + Charms.DeepFocus) ? 60 : 30;
            damageEnemies.attackType = AttackTypes.Spell;
            damageEnemies.ignoreInvuln = false;
            damageEnemies.enabled = true;
            this.PlayAudio((AudioClip)_pvControl.GetAction<AudioPlayerOneShotSingle>("Focus Burst", 8).audioClip.Value, 0, 1.5f);


            // Spawn additional things
            if (_pd.GetBool("equippedCharm_" + Charms.SporeShroom))
            {
                if (_pd.GetBool("equippedCharm_" + Charms.DefendersCrest))
                {
                    if (PaleCourtCharms.Settings.upgradedCharm_10)
                    {
                        Instantiate(_hc.GetComponent<RoyalAura>().dungCloud, transform.position, Quaternion.identity).SetActive(true);
                    }
                    else
                    {
                        Instantiate(PaleCourtCharms.preloadedGO["DungCloud"], transform.position, Quaternion.identity).SetActive(true);
                    }
                }
                else
                {
                    Instantiate(PaleCourtCharms.preloadedGO["SporeCloud"], transform.position, Quaternion.identity).SetActive(true);
                }
            }

            yield return new WaitForSeconds(.11f);
            blastCollider.enabled = false;
            yield return new WaitForSeconds(0.69f);

            Destroy(_blast);
            Destroy(_focusLines);

            try
            {
                LamentControl.markedEnemies.RemoveAt(LamentControl.markedEnemies.IndexOf(gameObject));
                Destroy(gameObject.GetComponent<Afflicted>());
            }
            catch (NullReferenceException e)
            {
            }
        }
    }
}