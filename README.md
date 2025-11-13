## Table Of Contents

<details>
<summary>Details</summary>

  - [Introduction](#Introduction)
  - [Getting Started](#GettingStarted)
  - [Simple Room Placement](#SimpleRoomPlacement)
  - [BSP](#BSP)
  - [Cellular Automata](#CellularAutomata)
  - [Noise Generator](#NoiseGenerator)

</details>

## Introduction
Gen-Proc-Week-1 est un repo contenant le nécessaire de base afin de  customiser des générations procédurales de base.

## Getting Started
Pour démarrer il faudra mettre un ```ProceduralGridGenerator``` dans la scène.  
<img src="Documentation/img1.png?raw=true"/>  
Ensuite, il vous faudra créer un nouveau script dérivant de la classe ```ProceduralGenerationMethod```.  
Faites votre génération procédurale dans ce dernier et créez un asset de ce script.  
Il vous faudra rentrer votre logique dans cette fonction :  
```
protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
{
    //Définition des variables

    // Check for cancellation
    cancellationToken.ThrowIfCancellationRequested();

    // Logique du code

    // Waiting between steps to see the result.
    await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
}
```
<img src="Documentation/asset.png?raw=true"/>  
Enfin, références cet asset dans votre ```ProceduralGridGenerator``` et vous aurez fini l'installation.
<img src="Documentation/endsetup.png?raw=true"/>

## Simple Room Placement
Ce script commence par générer des salles et les placer si elle n'overlapent pas d'autres salles.  
Ensuite il va générer des couloirs pour relier les salles de la plus ancienne créée à la plus récente. Les couloirs évitent et coutournent les salles.  
<img src="Documentation/Simpleroomplacement.png?raw=true"/>

## BSP
Ce script va également générer des salles au début mais avec un système de nodes. Il découpe la map jusqu'à atteindre le nombre de nodes enfants demandé. Une fois le nombre de nodes enfants atteint il va s'arrêter et instancier des salles à ces positions.
Ensuite il va générer des couloirs pour relier les salles entre les nodes les plus proches. Les couloirs évitent et coutournent les salles. (même fonction d'évitement que le script précédent)  
<img src="Documentation/BSP.png?raw=true"/>

## Cellular Automata
Pour ce script on change des salles. Générons du terrain maintenant. D'abord on commence par du noise simple. Juste un true ou false pour chaque case de la map. Ensuite on itère, si on respecte certaines conditions (ici +- de 4 cellules true autour de celle qu'on regarde). Si la condition est respectée on passe la cellule qu'on regarde en true, sinon on la passe en false. On fait cette boucle pour chaque case de notre map.  
Lorsque la taille de la map depasse 250x250 un autre layer de tiles sera ajouté. Cela rend plus long l'instanciation mais fait gagner beaucoup de temps sur les changements de tiles. C'est donc un ajout rentable sur les grosses grilles, d'où la taille minimum à 250x250.  
<img src="Documentation/Cellularautomata.png?raw=true"/>  
<img src="Documentation/CellularAutomataSecond.png?raw=true"/>  

## Noise Generator  
De nouveau une génération de terrain mais bien plus précise. On passera par la librairie FastNoiseLite afin de générer notre bruit. On le créé, lui assigne plusieurs paramètres et la librairie fait le reste il ne nous reste qu'à instancier nos tiles en fonction du résultat.
<img src="Documentation/NoiseGenerator.png?raw=true"/>  

## Advanced Noise Generator  
De nouveau une génération de terrain mais encore affinée par rapport au noise simple. On passera toujours par la librairie FastNoiseLite afin de générer notre bruit. Mais cette fois ci on en génère 3. Un pour la hauteur, pour l'humidité et pour la chaleur de la map afin de varier les biomes.On leur assigne différents paramètres et la librairie fait le reste il ne nous reste qu'à instancier nos tiles en fonction du résultat global des 3 noises combiné.  
<img src="Documentation/AdvancedNoiseGenerator.png?raw=true"/>
