// RealityOS Unreal Engine Companion Demonstration
// Maps to Course Syllabus: 21CSE353T - Unit IV: Unreal Engine and Techniques

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Components/WidgetComponent.h"
#include "Components/BoxComponent.h"
#include "RealityOSPanel.generated.h"

UENUM(BlueprintType)
enum class EUEPanelType : uint8
{
    Notes UMETA(DisplayName = "Notes"),
    TaskBoard UMETA(DisplayName = "Task Board"),
    Information UMETA(DisplayName = "Information"),
    ObjectViewer UMETA(DisplayName = "3D Object Viewer")
};

UCLASS()
class REALITYOSCOMPANION_API ARealityOSPanel : public AActor
{
    GENERATED_BODY()
    
public:	
    ARealityOSPanel();

protected:
    virtual void BeginPlay() override;

public:	
    virtual void Tick(float DeltaTime) override;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "RealityOS|Components")
    USceneComponent* RootSceneComponent;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "RealityOS|Components")
    UStaticMeshComponent* BackingMeshComponent;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "RealityOS|Components")
    UWidgetComponent* SpatialWidgetComponent;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "RealityOS|Components")
    UBoxComponent* InteractionBounds;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "RealityOS|Properties")
    EUEPanelType PanelType;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "RealityOS|Properties")
    FText PanelTitle;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "RealityOS|State")
    bool bIsSelected;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "RealityOS|State")
    bool bIsHovered;

    UFUNCTION(BlueprintCallable, Category = "RealityOS|Interaction")
    void OnSelect();

    UFUNCTION(BlueprintCallable, Category = "RealityOS|Interaction")
    void OnDeselect();

    UFUNCTION(BlueprintCallable, Category = "RealityOS|Interaction")
    void OnHoverEnter();

    UFUNCTION(BlueprintCallable, Category = "RealityOS|Interaction")
    void OnHoverExit();

    UFUNCTION(BlueprintCallable, Category = "RealityOS|Manipulation")
    void MoveTo(FVector TargetWorldLocation);

    UFUNCTION(BlueprintCallable, Category = "RealityOS|Manipulation")
    void RotateBy(FRotator DeltaRotation);

    UFUNCTION(BlueprintCallable, Category = "RealityOS|Manipulation")
    void ScaleBy(float ScaleMultiplier);

private:
    FVector DesiredLocation;
    FRotator DesiredRotation;
    FVector DesiredScale;

    UPROPERTY(EditDefaultsOnly, Category = "RealityOS|Kinematics")
    float InterpolationSpeed = 12.0f;
};

