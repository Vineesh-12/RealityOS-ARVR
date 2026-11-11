// RealityOS Unreal Engine Companion Demonstration
// Implementation of 3D Spatial Panel Actor

#include "RealityOSPanel.h"
#include "Kismet/KismetMathLibrary.h"

ARealityOSPanel::ARealityOSPanel()
{
    PrimaryActorTick.bCanEverTick = true;

    RootSceneComponent = CreateDefaultSubobject<USceneComponent>(TEXT("RootScene"));
    RootComponent = RootSceneComponent;

    BackingMeshComponent = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("BackingMesh"));
    BackingMeshComponent->SetupAttachment(RootComponent);

    SpatialWidgetComponent = CreateDefaultSubobject<UWidgetComponent>(TEXT("SpatialWidget"));
    SpatialWidgetComponent->SetupAttachment(RootComponent);
    SpatialWidgetComponent->SetWidgetSpace(EWidgetSpace::World);
    SpatialWidgetComponent->SetDrawSize(FVector2D(800.0f, 600.0f));

    InteractionBounds = CreateDefaultSubobject<UBoxComponent>(TEXT("InteractionBounds"));
    InteractionBounds->SetupAttachment(RootComponent);
    InteractionBounds->SetBoxExtent(FVector(40.0f, 40.0f, 2.0f));
    InteractionBounds->SetCollisionProfileName(TEXT("BlockAllDynamic"));

    PanelType = EUEPanelType::Notes;
    PanelTitle = FText::FromString(TEXT("Unreal Spatial Panel"));
    bIsSelected = false;
    bIsHovered = false;
}

void ARealityOSPanel::BeginPlay()
{
    Super::BeginPlay();
    DesiredLocation = GetActorLocation();
    DesiredRotation = GetActorRotation();
    DesiredScale = GetActorScale3D();
}

void ARealityOSPanel::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);

    // Smooth Kinematic Interpolation (VInterpTo, RInterpTo)
    FVector NewLoc = FMath::VInterpTo(GetActorLocation(), DesiredLocation, DeltaTime, InterpolationSpeed);
    FRotator NewRot = FMath::RInterpTo(GetActorRotation(), DesiredRotation, DeltaTime, InterpolationSpeed);
    FVector NewScale = FMath::VInterpTo(GetActorScale3D(), DesiredScale, DeltaTime, InterpolationSpeed);

    SetActorLocationAndRotation(NewLoc, NewRot);
    SetActorScale3D(NewScale);
}

void ARealityOSPanel::OnSelect()
{
    bIsSelected = true;
}

void ARealityOSPanel::OnDeselect()
{
    bIsSelected = false;
}

void ARealityOSPanel::OnHoverEnter()
{
    bIsHovered = true;
}

void ARealityOSPanel::OnHoverExit()
{
    bIsHovered = false;
}

void ARealityOSPanel::MoveTo(FVector TargetWorldLocation)
{
    DesiredLocation = TargetWorldLocation;
}

void ARealityOSPanel::RotateBy(FRotator DeltaRotation)
{
    DesiredRotation = UKismetMathLibrary::ComposeRotators(DesiredRotation, DeltaRotation);
}

void ARealityOSPanel::ScaleBy(float ScaleMultiplier)
{
    FVector Scaled = DesiredScale * ScaleMultiplier;
    Scaled.X = FMath::Clamp(Scaled.X, 0.4f, 3.0f);
    Scaled.Y = FMath::Clamp(Scaled.Y, 0.4f, 3.0f);
    Scaled.Z = FMath::Clamp(Scaled.Z, 0.4f, 3.0f);
    DesiredScale = Scaled;
}
