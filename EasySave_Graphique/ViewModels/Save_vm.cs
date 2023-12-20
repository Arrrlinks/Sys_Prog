﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using EasySave_Graphique.Models;
using Program.Models;
using System.Threading;
using Newtonsoft.Json;

namespace EasySave_Graphique.ViewModels;

public class Save_vm : Base_vm
{
    private save_m _saveM;
    
    private int select;
    

    private state_m _state;
    
    public RelayCommand LaunchCommand { get; set; }
    public RelayCommand SelectAllCommand { get; set; }
    public RelayCommand StartSaveCommand { get; set; }
    public RelayCommand PauseSaveCommand { get; set; }
    public RelayCommand StopSaveCommand { get; set; }
    public ObservableCollection<backup_m> Backups { get; set; } //list of backups that update UI
    
    private backup_m _selectedBackupM;
    //builder
    public Save_vm()
    {
        int select = 0;

        _saveM = new save_m();
        
        LaunchCommand = new RelayCommand(execute => Save());
        SelectAllCommand = new RelayCommand(execute => SelectAll());
        StartSaveCommand = new RelayCommand(StartSave);
        PauseSaveCommand = new RelayCommand(obj => PauseSave(obj));
        StopSaveCommand = new RelayCommand(obj => StopSave(obj));
        
        _state = new state_m();
        Backups = _state.GetBackupsFromStateFile();


        Modify_vm.BackupUpdated += UpdateSaveMenu;
        _saveM.SaveUpdated += UpdateSaveMenu;
    }

    private void PauseSave(object obj)
    {
        if (obj is backup_m backup)
        {
            if (backup.State != "Aborted")
            {
                bool isPaused = !_state.RetrieveValueFromStateFile(backup.Name, "IsPaused");
                backup.IsPaused = isPaused;
                backup.State = backup.IsPaused ? "Paused" : "Active";
                string newStatus = backup.IsPaused ? "Paused" : "Active";
                _state.ModifyJsonFile("../../../state.json", backup.Name, "IsPaused", isPaused);
                _state.ModifyJsonFile("../../../state.json", backup.Name, "State", newStatus);
                if (backup.IsPaused)
                {
                    _saveM.PauseSelectedSave(backup);
                }
                else
                {

                    _saveM.ResumeSelectedSave(backup);
                }
            }
            UpdateSaveMenu();
        }
    }

    private void StopSave(object obj)
    {
        if (obj is backup_m backup)
        {
            backup.State = "Aborted";
            _saveM.PauseSelectedSave(backup);
            _state.ModifyJsonFile("../../../state.json", backup.Name, "IsPaused", true);
            _state.ModifyJsonFile("../../../state.json", backup.Name, "State", "Aborted");
            UpdateSaveMenu();
        }
    }
    
    private void StartSave(object obj)
    {
        if (obj is backup_m backup)
        {
            Thread thread = new Thread(() =>_saveM.SaveLaunch(backup.Source, backup.Target, backup.Name));
            thread.Start();
        }
    }

    private void UpdateSaveMenu()
    {
        Backups.Clear();
        ObservableCollection<backup_m> updatedBackups = _state.GetBackupsFromStateFile();
        foreach (var backup in updatedBackups)
        {
            Backups.Add(backup);
        }
    }

    //methods
    public backup_m SelectedBackupM
    {
        get { return _selectedBackupM; }
        set
        {
            _selectedBackupM = value;
            OnPropertyChanged();
        }
    }
    
    private void Save()
    {
        foreach (var backup in Backups)
        {
            if (backup.Selected)
            {
                Thread thread = new Thread(() =>_saveM.SaveLaunch(backup.Source, backup.Target, backup.Name));
                thread.Start();
            }
        }
    }

    private void SelectAll()
    {
        foreach (var backup in Backups)
        {
            if (backup.Selected)
            {
                backup.Selected = false;
            }
            else
            {
                backup.Selected = true;
            }
        }
    }
}