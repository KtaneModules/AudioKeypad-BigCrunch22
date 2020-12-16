using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class AudioKeypadScript : MonoBehaviour
{
	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;
	
	public KMSelectable[] Buttons;
	public TextMesh[] Text;
	public KMSelectable PlayButton;
	public AudioSource Sound;
	public AudioClip[] SFX;
	
    // Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool ModuleSolved;
	
	string[] Choices = {"!", ",", "%", "-", "&", ".", "(", "/", ")", "?", "+", "@"};
	string[] ShuffledChoices = {"!", ",", "%", "-", "&", ".", "(", "/", ")", "?", "+", "@"};
	string CorrectAnswer = "";
	
	void Awake()
    {
		moduleId = moduleIdCounter++;
		for (int k = 0; k < 4; k++)
        {
			int PressInt = k;
            Buttons[PressInt].OnInteract += delegate
            {
				Buttons[PressInt].GetComponentInChildren<Animator>().SetTrigger("PushTrigger");
                Moving(PressInt);
                return false;
            };
		}
		PlayButton.OnInteract += delegate
		{
			PlayButton.GetComponentInChildren<Animator>().SetTrigger("PushTrigger");
			PlayMusic();
			return false;
		};
	}
	
	void Start()
	{
		ShuffledChoices.Shuffle();
		for (int x = 0; x < 4; x++)
		{
			Text[x].text = ShuffledChoices[x];
		}
		int ButtonPress = UnityEngine.Random.Range(0,4);
		CorrectAnswer = ShuffledChoices[ButtonPress];
		Debug.LogFormat("[Audio Keypad #{0}] The chosen music by the module: {1}", moduleId, SFX[Array.IndexOf(Choices, CorrectAnswer)].name);
		Debug.LogFormat("[Audio Keypad #{0}] The correct button to press (in reading order): Button {1}", moduleId, (ButtonPress + 1).ToString());
	}
	
	void PlayMusic()
	{
		PlayButton.AddInteractionPunch(0.1f);
		if (!ModuleSolved)
		{
			if (Sound.isPlaying)
			{
				Sound.Stop();
			}
			
			else
			{
				Sound.clip = SFX[Array.IndexOf(Choices, CorrectAnswer)];
				Sound.Play();
			}
		}
		
		else
		{
			Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
		}
	}
	
	void Moving (int Movement)
	{
		Buttons[Movement].AddInteractionPunch(0.1f);
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
		if (!ModuleSolved)
		{
			if (Text[Movement].text == CorrectAnswer)
			{
				Debug.LogFormat("[Audio Keypad #{0}] You pressed Button {1}. That was correct. Module solved", moduleId, (Movement + 1).ToString());
				if (Sound.isPlaying)
				{
					Sound.Stop();
				}
				Module.HandlePass();
				Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
				ModuleSolved = true;
			}
			
			else
			{
				if (Sound.isPlaying)
				{
					Sound.Stop();
				}
				Debug.LogFormat("[Audio Keypad #{0}] You pressed Button {1}. That was incorrect. The module resets.", moduleId, (Movement + 1).ToString());
				Module.HandleStrike();
				Start();
			}
		}
	}
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To press the play button above the keypad, use the command !{0} play | To press a button on the keypad (in reading order), use the command !{0} press [1-4]";
    #pragma warning restore 414
	
	string[] ValidNumbers = {"1", "2", "3", "4"};
	IEnumerator ProcessTwitchCommand(string command)
    {
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(command, @"^\s*play\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
			PlayButton.OnInteract();
        }
		
		if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
			yield return null;
			if (parameters.Length != 2)
			{
				yield return "sendtochaterror Invalid parameter length. The command was not processed.";
				yield break;
			}
		      
			if (!parameters[1].EqualsAny(ValidNumbers))
			{
				yield return "sendtochaterror Invalid button position sent. The command was not processed.";
				yield break;
			}
			Buttons[Int32.Parse(parameters[1]) - 1].OnInteract();
        }
	}
}
